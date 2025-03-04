#if DEBUG

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Channels;
using Microsoft.AspNetCore.Components;

using Blazor.Server;

[assembly: System.Reflection.Metadata.MetadataUpdateHandlerAttribute(typeof(TailwindHotReloadService))]

namespace Blazor.Server;

/// <summary>
/// This is a special hook for dotnet-watch, which notifies the app if hot reload happens,
/// and additional logic for watching over *.css files.
/// The code checks if there are blazor component and css related changes that must trigger tailwind rebuild.
/// </summary>
public class TailwindHotReloadService : BackgroundService
{
    private static readonly string Executable;
    private const string Args = "@tailwindcss/cli -i ./app.css -o ./wwwroot/styles.css";
    protected static string Command;

    private static Channel<DateTimeOffset> RebuildNotificationChannel { get; } =
        Channel.CreateBounded<DateTimeOffset>(
            new BoundedChannelOptions(capacity: 1)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleWriter = false,
                SingleReader = true,
                AllowSynchronousContinuations = false
            }
        );

    private static readonly FileSystemWatcher Watcher;

    static TailwindHotReloadService()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Executable = "npx.cmd";
        }
        else
        {
            Executable = "npx"; // Not confirmed that it works on Linux or MacOS. To be improved.
        }

        Command = $"{Executable} {Args}";

        // We have to watch *.css files manually because dotnet-watch doesn't trigger ClearCache and UpdateApplication methods
        // if you do a change in *.css files
        Watcher = new FileSystemWatcher("./", "*.css");
        // To not infinitely reload if wwwroot/styles.css is changed.
        // If subdirectories are required then need to be reworked
        Watcher.IncludeSubdirectories = false;
        Watcher.Changed += HandleCssFileChanges;
        Watcher.EnableRaisingEvents = true;
    }

    private static void HandleCssFileChanges(object sender, FileSystemEventArgs args)
    {
        RebuildNotificationChannel.Writer.TryWrite(TimeProvider.System.GetUtcNow());
    }

    internal static void ClearCache(Type[]? types)
    {
        ExecuteTailwindCssIfRequired(types);
    }

    internal static void UpdateApplication(Type[]? types)
    {
        ExecuteTailwindCssIfRequired(types);
    }

    private static void ExecuteTailwindCssIfRequired(Type[]? types)
    {
        if (types?.Any(t => t.IsSubclassOf(typeof(ComponentBase))) != true)
        {
            return;
        }

        RebuildNotificationChannel.Writer.TryWrite(TimeProvider.System.GetUtcNow());
    }


    public static void RequestRebuild(DateTimeOffset rebuildRequestedAt)
    {
        RebuildNotificationChannel.Writer.TryWrite(rebuildRequestedAt);
    }


    private readonly ILogger<TailwindHotReloadService> _logger;
    private readonly TimeProvider _timeProvider;

    public TimeSpan TailwindCssProcessTimeout { get; set; } = TimeSpan.FromSeconds(10);

    public TailwindHotReloadService(
        ILogger<TailwindHotReloadService> logger,
        TimeProvider timeProvider
    )
    {
        _logger = logger;
        _timeProvider = timeProvider;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        TimeSpan debounceInterval = TimeSpan.FromMilliseconds(500);
        TimeSpan checkInterval = TimeSpan.FromMilliseconds(100);

        DateTimeOffset rebuildAfter = DateTimeOffset.MaxValue;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (RebuildNotificationChannel.Reader.TryRead(out DateTimeOffset rebuildRequestedAt))
                {
                    DateTimeOffset newRebuildAfter = rebuildRequestedAt.Add(debounceInterval);
                    if (rebuildAfter == DateTimeOffset.MaxValue || newRebuildAfter > rebuildAfter)
                    {
                        rebuildAfter = newRebuildAfter;
                    }
                }

                DateTimeOffset now = _timeProvider.GetUtcNow();
                if (rebuildAfter < now)
                {
                    // Expected that this method never fails
                    await ExecuteTailwindCssAsync(stoppingToken);
                    rebuildAfter = DateTimeOffset.MaxValue;
                }
                else
                {
                    await Task.Delay(checkInterval, stoppingToken);
                }
            }
            catch (TaskCanceledException)
            {
                // ignore
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected error");
            }
        }
    }

    protected virtual IProcess StartTailwindCssProcess(CancellationToken stoppingToken) =>
        new ProcessWrapper(Process.Start(Executable, Args));

    protected virtual async Task ExecuteTailwindCssAsync(CancellationToken stoppingToken)
    {
        try
        {
            TimeSpan timeout = TailwindCssProcessTimeout;

            IProcess process = StartTailwindCssProcess(stoppingToken);

            await Task.WhenAny(
                process.WaitForExitAsync(stoppingToken),
                Task.Delay(timeout, stoppingToken)
            );

            if (!process.HasExited)
            {
                _logger.LogError("The command '{Command}' has timed out after {Timeout}. Killing!", Command, timeout);
                process.Kill();
            }

            if (process.ExitCode != 0)
            {
                _logger.LogError("'{Command}' finished with non-zero exit code '{ProcessExitCode}'", Command, process.ExitCode);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during execution of '{Command}'", Command);
        }
    }
}

public interface IProcess
{
    void Kill();
    bool HasExited { get; }
    int ExitCode { get; }
    Task WaitForExitAsync(CancellationToken cancellationToken);
}

public sealed class ProcessWrapper : IProcess
{
    private readonly Process _process;

    public ProcessWrapper(Process process)
    {
        _process = process;
    }

    public bool HasExited => _process.HasExited;
    public int ExitCode => _process.ExitCode;

    public Task WaitForExitAsync(CancellationToken cancellationToken) => _process.WaitForExitAsync(cancellationToken);

    public void Kill() => _process.Kill();
}

#endif
