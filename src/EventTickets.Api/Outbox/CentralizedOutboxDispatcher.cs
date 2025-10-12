// src/EventTickets.Api/Outbox/CentralizedOutboxDispatcher.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using EventTickets.Shared.Integration;
using EventTickets.Ticketing.Infrastructure;
using EventTickets.Payments.Infrastructure;

namespace EventTickets.Api.Outbox;

public sealed class CentralizedOutboxDispatcher : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CentralizedOutboxDispatcher> _logger;

    public CentralizedOutboxDispatcher(IServiceScopeFactory scopeFactory, ILogger<CentralizedOutboxDispatcher> logger)
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
        
        // Processa eventi da entrambi i BC
        var ticketingProcessed = await ProcessTicketingEventsAsync(scope, take, ct);
        var paymentsProcessed = await ProcessPaymentsEventsAsync(scope, take, ct);
        
        return ticketingProcessed + paymentsProcessed;
    }

    private async Task<int> ProcessTicketingEventsAsync(IServiceScope scope, int take, CancellationToken ct)
    {
        var ticketingDb = scope.ServiceProvider.GetRequiredService<TicketingDbContext>();
        
        var batch = await ticketingDb.OutboxMessages
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
                _logger.LogError(ex, "Failed to process ticketing outbox message {Id}", msg.Id);
            }
        }
        
        await ticketingDb.SaveChangesAsync(ct);
        return batch.Count;
    }

    private async Task<int> ProcessPaymentsEventsAsync(IServiceScope scope, int take, CancellationToken ct)
    {
        var paymentsDb = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
        
        var batch = await paymentsDb.OutboxMessages
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
        
        await paymentsDb.SaveChangesAsync(ct);
        return batch.Count;
    }

    private async Task ProcessEventAsync(IServiceScope scope, dynamic msg, CancellationToken ct)
    {
        var eventType = FindEventType(msg.Type);
        if (eventType == null)
        {
            _logger.LogWarning("Unknown event type: {Type}", (string)msg.Type);
            return;
        }

        var integrationEvent = JsonSerializer.Deserialize(msg.Content, eventType) as IntegrationEvent;
        if (integrationEvent == null)
        {
            _logger.LogWarning("Failed to deserialize event: {Type}", (string)msg.Type);
            return;
        }

        // Trova e invoca l'handler appropriato
        var handlerType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
        var handler = scope.ServiceProvider.GetService(handlerType);
        
        if (handler != null)
        {
            var method = handlerType.GetMethod("HandleAsync");
            await (Task)method!.Invoke(handler, new object[] { integrationEvent, ct })!;
            _logger.LogInformation("Processed integration event {Type} ({Id})", (string)msg.Type, (Guid)msg.Id);
        }
        else
        {
            _logger.LogWarning("No handler found for event type: {Type}", (string)msg.Type);
        }
    }

    private Type? FindEventType(string typeName)
    {
        // Prima prova con Type.GetType
        var type = Type.GetType(typeName);
        if (type != null) return type;

        // Se non trova, cerca in tutti gli assembly caricati
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            type = assembly.GetType(typeName);
            if (type != null) return type;
        }

        return null;
    }
}