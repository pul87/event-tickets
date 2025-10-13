
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using EventTickets.Shared.Integration;
using EventTickets.Shared.Outbox;
namespace EventTickets.Payments.Infrastructure.Outbox;

public sealed class PaymentsOutboxDispatcher : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PaymentsOutboxDispatcher> _logger;

    public PaymentsOutboxDispatcher(IServiceScopeFactory scopeFactory, ILogger<PaymentsOutboxDispatcher> logger)
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
                _logger.LogError(ex, "Payments outbox dispatcher error");
                await Task.Delay(delay, ct);
            }
        }
    }

    private async Task<int> ProcessBatchAsync(int take, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
        
        var batch = await db.OutboxMessages
            .Where(x => x.ProcessedOnUtc == null)
            .OrderBy(x => x.OccurredOnUtc)
            .Take(take)
            .ToListAsync(ct);

        if (batch.Count == 0) return 0;

        foreach (var msg in batch)
        {
            try
            {
                await ProcessEventAsync(scope, msg, ct);
                msg.ProcessedOnUtc = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                msg.Error = ex.Message;
                _logger.LogError(ex, "Failed to process payments outbox message {Id}", msg.Id);
            }
        }
        
        await db.SaveChangesAsync(ct);
        return batch.Count;
    }

    private async Task ProcessEventAsync(IServiceScope scope, OutboxMessage msg, CancellationToken ct)
    {
        var eventType = FindEventType(msg.Type);
        if (eventType == null)
        {
            _logger.LogWarning("Unknown event type: {Type}", msg.Type);
            return;
        }

        var integrationEvent = JsonSerializer.Deserialize(msg.Content, eventType) as IntegrationEvent;
        if (integrationEvent == null)
        {
            _logger.LogWarning("Failed to deserialize event: {Type}", msg.Type);
            return;
        }

        // Trova e invoca l'handler appropriato
        var handlerType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
        var handler = scope.ServiceProvider.GetService(handlerType);
        
        if (handler != null)
        {
            var method = handlerType.GetMethod("HandleAsync");
            await (Task)method!.Invoke(handler, new object[] { integrationEvent, ct })!;
            _logger.LogInformation("Processed payments integration event {Type} ({Id})", msg.Type, msg.Id);
        }
        else
        {
            _logger.LogWarning("No handler found for event type: {Type}", msg.Type);
        }
    }

    private Type? FindEventType(string typeName)
    {
        var type = Type.GetType(typeName);
        if (type != null) return type;

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            type = assembly.GetType(typeName);
            if (type != null) return type;
        }

        return null;
    }
}