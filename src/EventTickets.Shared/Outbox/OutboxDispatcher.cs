using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using EventTickets.Shared.IntegrationEvents;

namespace EventTickets.Shared.Outbox;

public abstract class OutboxDispatcher<TDbContext> : BackgroundService
    where TDbContext : class
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger _logger;

    protected OutboxDispatcher(
        IServiceScopeFactory scopeFactory, 
        ILogger logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

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
                _logger.LogError(ex, "Outbox dispatcher error for {DbContext}", typeof(TDbContext).Name);
                await Task.Delay(delay, ct);
            }
        }
    }

    private async Task<int> ProcessBatchAsync(int take, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TDbContext>();
        
        var batch = await GetOutboxMessagesAsync(db, take, ct);
        if (batch.Count == 0) return 0;

        foreach (var msg in batch)
        {
            try
            {
                await ProcessMessageAsync(scope, msg, ct);
                await MarkAsProcessedAsync(db, msg);
            }
            catch (Exception ex)
            {
                await MarkAsErrorAsync(db, msg, ex.Message);
                _logger.LogError(ex, "Failed to process outbox message {Id}", msg.Id);
            }
        }
        
        await SaveChangesAsync(db, ct);
        return batch.Count;
    }

    private async Task ProcessMessageAsync(IServiceScope scope, OutboxMessage msg, CancellationToken ct)
    {
        var integrationEvent = DeserializeEventAsync(msg.Content, msg.Type);
        if (integrationEvent == null) return;

        await ProcessEventAsync(scope, integrationEvent);
    }

    private IntegrationEvent? DeserializeEventAsync(string content, string eventType)
    {
        var type = FindEventType(eventType);
        if (type == null)
        {
            _logger.LogWarning("Unknown event type: {Type}", eventType);
            return null;
        }

        var integrationEvent = System.Text.Json.JsonSerializer.Deserialize(content, type, OutboxJsonOptions.Default) as IntegrationEvent;
        if (integrationEvent == null)
        {
            _logger.LogWarning("Failed to deserialize event: {Type}", eventType);
            return null;
        }

        return integrationEvent;
    }

    private async Task ProcessEventAsync(IServiceScope scope, IntegrationEvent integrationEvent)
    {
        var eventType = integrationEvent.GetType();
        var handlerType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
        var handler = scope.ServiceProvider.GetService(handlerType);
        
        if (handler == null)
        {
            _logger.LogWarning("No handler found for event type: {Type}", eventType.Name);
            return;
        }

        var method = handlerType.GetMethod("HandleAsync");
        await (Task)method!.Invoke(handler, new object[] { integrationEvent, CancellationToken.None })!;
        
        _logger.LogInformation("Processed integration event {Type}", eventType.Name);
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

    // Metodi astratti da implementare nelle classi concrete
    protected abstract Task<List<OutboxMessage>> GetOutboxMessagesAsync(TDbContext db, int take, CancellationToken ct);
    protected abstract Task MarkAsProcessedAsync(TDbContext db, OutboxMessage msg);
    protected abstract Task MarkAsErrorAsync(TDbContext db, OutboxMessage msg, string error);
    protected abstract Task SaveChangesAsync(TDbContext db, CancellationToken ct);
}