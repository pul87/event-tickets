using EventTickets.Shared.IntegrationEvents;

namespace EventTickets.Shared.Outbox;

public interface IOutbox
{
    Task EnqueueAsync(IntegrationEvent @event, CancellationToken ct = default);
}