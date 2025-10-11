using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EventTickets.Ticketing.Infrastructure.Outbox;

public sealed class OutboxDispatcher : BackgroundService
{

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxDispatcher> _logger;

    public OutboxDispatcher(IServiceScopeFactory scopeFactory, ILogger<OutboxDispatcher> logger)
        => (_scopeFactory, _logger) = (scopeFactory, logger);

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var delay = TimeSpan.FromSeconds(1);
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var processed = await ProcessBatchAsync(50, ct);
                if (processed == 0)
                    await Task.Delay(delay, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox dispatcher error");
                await Task.Delay(delay, ct);
            }
        }
    }

    private async Task<int> ProcessBatchAsync(int take, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TicketingDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<OutboxDispatcher>>();

        var batch = await db.OutboxMessages
            .Where(x => x.ProcessedOnUtc == null)
            .OrderBy(x => x.OccurredOnUtc)
            .Take(take)
            .ToListAsync();

        if (batch.Count == 0) return 0;

        foreach (var msg in batch)
        {
            try
            {
                logger.LogInformation("Dispatching integration event {Type} ({Id})", msg.Type, msg.Id);
                msg.ProcessedOnUtc = DateTime.UtcNow;
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                // shutdown normale: esci silenziosamente
                break;
            }
            catch (Exception ex)
            {
                msg.Error = ex.Message;
                logger.LogError(ex, "Failed to dispatch outbox message {Id}", msg.Id);
            }
        }
        await db.SaveChangesAsync();
        return batch.Count;
    }

}