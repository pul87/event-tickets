using EventTickets.Shared.Integration;

namespace EventTickets.Shared.Outbox;

public interface IOutbox
{
    Task EnqueueAsync(IntegrationEvent @event, CancellationToken ct = default);
}