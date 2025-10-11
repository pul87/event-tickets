using EventTickets.Shared.Integration;

namespace EventTickets.Ticketing.Application.IntegrationEvents;

public sealed record ReservationPlacedIntegrationEvent(Guid ReservationId, Guid PerformanceId, int Quantity) 
    : IntegrationEvent;