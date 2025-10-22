namespace EventTickets.Shared.IntegrationEvents;
public interface IIntegrationEventHandler<in T> where T : IntegrationEvent
{
    Task HandleAsync(T @event, CancellationToken ct);
}
