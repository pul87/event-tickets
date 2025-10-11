using System.Text.Json;
using EventTickets.Shared.Integration;
using EventTickets.Ticketing.Application.Abstractions;

namespace EventTickets.Ticketing.Infrastructure.Outbox;

public sealed class EdOutbox : IOutbox
{
    private readonly TicketingDbContext _db;
    private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    public EdOutbox(TicketingDbContext db) => _db = db;

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