using DomainRelay.EFCore.Outbox.Dispatching;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DomainRelay.EFCore.Outbox.Hosting;

public sealed class OutboxDispatcherHostedService<TDbContext> : BackgroundService
    where TDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    private readonly OutboxDispatcher<TDbContext> _dispatcher;
    private readonly OutboxOptions _options;
    private readonly ILogger<OutboxDispatcherHostedService<TDbContext>> _logger;

    public OutboxDispatcherHostedService(
        OutboxDispatcher<TDbContext> dispatcher,
        OutboxOptions options,
        ILogger<OutboxDispatcherHostedService<TDbContext>> logger)
    {
        _dispatcher = dispatcher;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DomainRelay Outbox Dispatcher started (instance={Instance}).", _options.InstanceId);

        using var pollTimer = new PeriodicTimer(_options.PollingInterval);
        using var cleanupTimer = new PeriodicTimer(_options.CleanupInterval);

        var nextCleanupUtc = DateTime.UtcNow.Add(_options.CleanupInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Poll tick
                if (await pollTimer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
                {
                    var processed = await _dispatcher.DispatchOnceAsync(stoppingToken).ConfigureAwait(false);

                    if (_options.VerboseLogging && processed == 0)
                        _logger.LogDebug("Outbox poll tick: no messages processed.");

                    if (processed > 0)
                        _logger.LogDebug("Outbox processed {Count} messages.", processed);
                }

                // Cleanup (time-based, not timer-based to avoid drift issues)
                if (DateTime.UtcNow >= nextCleanupUtc)
                {
                    nextCleanupUtc = DateTime.UtcNow.Add(_options.CleanupInterval);

                    var deleted = await _dispatcher.CleanupAsync(stoppingToken).ConfigureAwait(false);
                    if (deleted > 0)
                        _logger.LogInformation("Outbox cleanup deleted {Count} processed messages.", deleted);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox dispatcher loop error.");
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken).ConfigureAwait(false);
            }
        }

        _logger.LogInformation("DomainRelay Outbox Dispatcher stopped (instance={Instance}).", _options.InstanceId);
    }
}
