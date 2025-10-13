using System.Text.Json;
using EventTickets.Shared.Integration;
using EventTickets.Shared.Outbox;
using EventTickets.Ticketing.Application.Abstractions;

namespace EventTickets.Ticketing.Infrastructure.Outbox;

public sealed class EfOutbox : IOutbox
{
    private readonly TicketingDbContext _db;

    public EfOutbox(TicketingDbContext db) => _db = db;

    public Task EnqueueAsync(IntegrationEvent @event, CancellationToken ct = default)
    {
        var msg = new OutboxMessage
        {
            Id = @event.Id,
            OccurredOnUtc = @event.OccurredOnUtc,
            Type = @event.EventType,
            Content = JsonSerializer.Serialize(@event, @event.GetType(), OutboxJsonOptions.Default)
        };
        _db.OutboxMessages.Add(msg);
        return Task.CompletedTask;
    }
}