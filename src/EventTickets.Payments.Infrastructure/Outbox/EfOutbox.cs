// src/EventTickets.Payments.Infrastructure/Outbox/EfOutbox.cs
using System.Text.Json;
using EventTickets.Shared.Integration;

namespace EventTickets.Payments.Infrastructure.Outbox;

public interface IOutbox
{
    Task EnqueueAsync(IntegrationEvent @event, CancellationToken ct = default);
}

public sealed class EfOutbox : IOutbox
{
    private readonly PaymentsDbContext _db;
    private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    public EfOutbox(PaymentsDbContext db) => _db = db;

    public Task EnqueueAsync(IntegrationEvent @event, CancellationToken ct = default)
    {
        var msg = new OutboxMessage
        {
            Id = @event.Id,
            OccurredOnUtc = @event.OccurredOnUtc,
            Type = @event.EventType,
            Content = JsonSerializer.Serialize(@event, @event.GetType(), _json)
        };

        _db.OutboxMessages.Add(msg);
        return Task.CompletedTask;
    }
}