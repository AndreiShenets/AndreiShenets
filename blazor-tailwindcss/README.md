# Integrating Tailwind CSS in Blazor with Hot Reload

It is always marvelous find that some problems are tried to be solved around the same time. 

Recently, I needed to do a lot of UI improvements in one of my projects. 
The project is not complex and using Blazor with Server side rendering because it is a fast way to deliver UI without requiring to set up the whole JS ecosystem.
The problem here that the approach works until some point because JS ecosystem has a lot of nice things and Tailwind CSS is one of them. 
I use it in my another project and together with vite and hot reload it works really great. 

According to current requirements UI does not require complex frontend stuff with a lot of user interaction 
so migrating to JS ecosystem with something like React + Vite + Tailwind sounds like an overkill. 
So I decided why not just use the only missing piece of the puzzle in the current setup - Tailwind CSS. 
Moreover, .NET has integrated hot reload with `dotnet watch` command. 

As I said it is always marvelous... Quite quickly I found a really nice and helpful article 
[Integrating Tailwind CSS in Blazor](https://timdeschryver.dev/blog/integrating-tailwind-css-in-blazor) from Tim Deschryver.

The article shows main steps of integration and that was a really nice starting point. Thanks, Tim!

There were just a few issues with it:
- The first one is that it was written for Tailwind CSS v3
- The second one is that there were no hot reload. Which I found out later.

The first issue is not a big deal because the official documentation of Tailwind CSS has a section with migration guide from v3 to v4.
And I already did the migration for my other React project.

After integrating Tailwind according to Tom's article I was excited to see that it works. 
I just ran `dotnet watch` as usual and found a strange behavior that adding of tailwind utilities doesn't change the UI most of the time but from time to time work.

It was unexpected as Tim wrote that `<Watch Include="./app.css" />` must be included into your `csproj`. I thought that should be it. 
So I spend some time trying to understand what I do wrong. 
After small amount of time I realized that the problem is that Tailwind compile command doesn't run with hot reload - html refreshes but css is not recompiled.

The reason for that is partially highlighted in this [issue](https://github.com/dotnet/aspnetcore/issues/33861) 
and together with other mentioned there documentation that gives a full picture. 
`dotnet watch` bypasses MSBuild most of the time and injects changes directly into your .NET host. 
It also has integration with your browser with injecting some middleware into middleware pipeline. 

Having the Tailwind was already a win but without constant hot reload you have to start two terminals and run two watch commands: one for the .NET app and one for tailwind.
That is completely not perfect. So I decided why not to fix it.

I started to investigate if there is a way to integrate hot reload in `dotnet watch`. 
I quickly found that you don't have neither control nor settings to integrate directly into watch functionality 
as Microsoft would like to ensure that this works as expected and really fast. 
I even checked the source code for some potential undocumented ways but I didn't find anything. 

At the same time, after second reading through comments in the issue mentioned above, 
I found that `MetadataUpdateHandlerAttribute` is mentioned as possible useful integration / extension point.
The documentation says that whenever watch detects a change it notifies the app by calling two static method of a class if it is marked with this attribute.
Microsoft uses this internally to notify the .NET host that caches should be invalidated.

OK, let's try to utilize this attribute.

```csharp
using Blazor.Server;

[assembly: System.Reflection.Metadata.MetadataUpdateHandlerAttribute(typeof(TailwindHotReloadService))]

namespace Blazor.Server;

public class TailwindHotReloadService : BackgroundService
{
    internal static void ClearCache(Type[]? types)
    {
        Console.WriteLine("Clearing cache");
    }

    internal static void UpdateApplication(Type[]? types)
    {
        Console.WriteLine("Updating application");
    }
}
```

The code just works, my app is notified and that is nice. Now I can run tailwind compile command with something like: 

```csharp
Process.Start(fileName: "npx", arguments: "@tailwindcss/cli -i ./app.css -o ./wwwroot/styles.css");
```

Not exactly! At first, it is not a terminal so `npx` must become `npx.cmd` on Windows and something other on Linux and MacOS.
and the second, it is not triggered when `css` files are changed! Although, `css` is watched according to `<Watch.../>` definition.

I though "OK, challenge accepted". I checked how Microsoft watches for changes, thanks .NET is open sourced, they just use `FileSystemWatcher`.

Then let's utilize another feature of .NET - Channels, I wanted to test them once in any case and that looks like a good use case, 
let's add `FileSystemWatcher` and `BackgroundService`, some flavor of protective programming and wrappers for testability. Done!

[Source code](https://github.com/AndreiShenets/AndreiShenets/tree/main/blazor-tailwindcss/SolutionExample/src/Blazor.Server/TailwindHotReloadService.cs)
```csharp
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
    private static readonly string Command;

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
            Executable = "npx"; // Not confirmed that it works on Linux or MaxOS. To be improved.
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
```

The code is not perfect, it has a lot of hardcoded stuff, it monitors only one entry point - `app.css`, it doesn't support subdirectories, 
BUT it WORKS and ready for copy-pasting into other projects. I even thought that it might be useful to create a NuGet package for it, but I refused this idea.

Each project has its own setup and making really nice package that can be easily integrated in any possible project for every possible .NET including future is quite a challenge.
As well as it means time-consuming support of the package. It is much easier to just copy-paste the code into your project and make it work for your case.

Because the code is not perfect, there might be issues with it. I know about at least a few:

- Sometimes 10 seconds is not enough to finish compilation. I am not sure why. 
  It might be that compilation is blocked by some other process like IDE or `dotnet watch`. 
  Sometimes it is either self-healing because next change is already scheduled or another change is required to trigger compilation.
- The hot reload is not instant. It takes sometime taking in account debouncing interval and all check intervals to reload the page. 
  But it is much faster than restarting the app.

There is a small fully working [SolutionExample](https://github.com/AndreiShenets/AndreiShenets/tree/main/blazor-tailwindcss/SolutionExample) to test it. Feel free to use it in your projects. Improvements are welcome.
