using EventTickets.Shared.Integration;

namespace EventTickets.Ticketing.Application.Abstractions;

public interface IOutbox
{
    Task EnqueueAsync(IntegrationEvent @event, CancellationToken ct = default);
}