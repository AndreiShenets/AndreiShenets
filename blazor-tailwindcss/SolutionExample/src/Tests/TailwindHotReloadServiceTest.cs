#if DEBUG

using Argon;
using Blazor.Server;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;

namespace Tests;

[NotInParallel]
public class TailwindHotReloadServiceTest
{
    [Test]
    public async Task Does_not_execute_tailwind_css_if_no_changes()
    {
        // Arrange
        var process = Substitute.For<IProcess>();
        var loggerCollector = new FakeLogCollector();
        var logger = new FakeLogger<TailwindHotReloadService>(loggerCollector);

        TailwindHotReloadServiceAccessor service = new (logger, TimeProvider.System, process);
        service.TailwindCssProcessTimeout = TimeSpan.FromMilliseconds(1);

        // Act
        await service.StartAsync(CancellationToken.None);

        // At least 3 checks should happen during this time
        await Task.Delay(333, CancellationToken.None);

        await service.StopAsync(CancellationToken.None);

        // Assert
        // Expected to see no executions and no logs
        var context =
            new
            {
                service.TailwindProcessCallCount,
                LogRecords = loggerCollector.GetSnapshot()
            };
        await Verify(context)
            .DontIgnoreEmptyCollections()
            .AddExtraSettings(
                settings => settings.DefaultValueHandling = DefaultValueHandling.Include
            );
    }

    [Test]
    public async Task Timeouts_if_process_takes_too_long()
    {
        // Arrange
        var process = Substitute.For<IProcess>();
        var loggerCollector = new FakeLogCollector();
        var logger = new FakeLogger<TailwindHotReloadService>(loggerCollector);

        TailwindHotReloadServiceAccessor service = new (logger, TimeProvider.System, process);

        TimeSpan initialTimeout = service.TailwindCssProcessTimeout;
        service.TailwindCssProcessTimeout = TimeSpan.FromSeconds(1);

        // Act
        await service.StartAsync(CancellationToken.None);

        TailwindHotReloadService.RequestRebuild(TimeProvider.System.GetUtcNow());

        // There is an internal timeout of 10 seconds to finish compilation, which reset to 1 second above.
        // 2 seconds should be enough to get it
        await Task.Delay(TimeSpan.FromSeconds(2), CancellationToken.None);

        // Emulating normal process exit
        process.HasExited.Returns(true);
        process.ExitCode.Returns(0);

        TailwindHotReloadService.RequestRebuild(TimeProvider.System.GetUtcNow());

        // Now checking that second request finished
        await Task.Delay(TimeSpan.FromSeconds(2), CancellationToken.None);

        await service.StopAsync(CancellationToken.None);

        // Assert

        // It is expected to see one  timeout error in log records and two executions
        var context =
            new
            {
                InitialTimeout = initialTimeout,
                service.TailwindProcessCallCount,
                LogRecords = loggerCollector.GetSnapshot()
            };
        await Verify(context);
    }

    [Test]
    public async Task Logs_if_process_tailwind_process_exited_with_non_zero_code()
    {
        // Arrange
        var process = Substitute.For<IProcess>();
        var loggerCollector = new FakeLogCollector();
        var logger = new FakeLogger<TailwindHotReloadService>(loggerCollector);

        TailwindHotReloadServiceAccessor service = new (logger, TimeProvider.System, process);
        service.TailwindCssProcessTimeout = TimeSpan.FromMilliseconds(100);

        // Emulating non-normal process exit
        process.HasExited.Returns(true);
        process.ExitCode.Returns(1234);

        // Act
        await service.StartAsync(CancellationToken.None);

        TailwindHotReloadService.RequestRebuild(TimeProvider.System.GetUtcNow());

        // It should take a bit less than debounce interval to process
        await Task.Delay(TimeSpan.FromMilliseconds(555), CancellationToken.None);

        await service.StopAsync(CancellationToken.None);

        // Assert

        // It is expected to see one non-normal exit error in logs and one execution
        var context =
            new
            {
                service.TailwindProcessCallCount,
                LogRecords = loggerCollector.GetSnapshot()
            };
        await Verify(context);
    }

    [Test]
    public async Task Debounces_update_requests()
    {
        // Arrange
        var process = Substitute.For<IProcess>();
        var loggerCollector = new FakeLogCollector();
        var logger = new FakeLogger<TailwindHotReloadService>(loggerCollector);

        TailwindHotReloadServiceAccessor service = new (logger, TimeProvider.System, process);
        service.TailwindCssProcessTimeout = TimeSpan.FromMilliseconds(100);

        // Emulating normal process exit
        process.HasExited.Returns(true);
        process.ExitCode.Returns(0);

        // Act
        await service.StartAsync(CancellationToken.None);

        DateTimeOffset spamServiceUntil = TimeProvider.System.GetUtcNow().AddSeconds(2);
        while (spamServiceUntil >= TimeProvider.System.GetUtcNow())
        {
            TailwindHotReloadService.RequestRebuild(TimeProvider.System.GetUtcNow());
            await Task.Delay(TimeSpan.FromMilliseconds(99), CancellationToken.None);
        }

        // It should take a bit less than debounce interval to process
        await Task.Delay(TimeSpan.FromMilliseconds(555), CancellationToken.None);

        await service.StopAsync(CancellationToken.None);

        // Assert

        // After spamming for two seconds with update request
        // I expect to see only one execution and no log entries
        var context =
            new
            {
                service.TailwindProcessCallCount,
                LogRecords = loggerCollector.GetSnapshot().Select(r => $"[{r.Timestamp:hh:mm:ss.fff}] {r.Message}")
            };

        await Verify(context)
            .DontIgnoreEmptyCollections()
            .AddExtraSettings(
                settings => settings.DefaultValueHandling = DefaultValueHandling.Include
            );
    }
}

/// <summary>
/// The class is a wrapper around <see cref="TailwindHotReloadService"/> that allows to inject a mocked process to prevent real rebuild calls.
/// It still allows to test an actual behavior.
/// </summary>
public sealed class TailwindHotReloadServiceAccessor : TailwindHotReloadService
{
    private readonly IProcess _tailwindProcess;

    public int TailwindProcessCallCount { get; private set; }

    public TailwindHotReloadServiceAccessor(
        ILogger<TailwindHotReloadService> logger,
        TimeProvider timeProvider,
        IProcess tailwindProcess
    )
        : base(logger, timeProvider)
    {
        _tailwindProcess = tailwindProcess;
    }

    protected override IProcess StartTailwindCssProcess(CancellationToken stoppingToken)
    {
        TailwindProcessCallCount++; // Shouldn't be a problem with parallel executions as there shouldn't be any
        return _tailwindProcess;
    }
}

#endif
