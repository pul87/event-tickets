using EventTickets.Shared.IntegrationEvents;

namespace EventTickets.Shared.IntegrationEvents;

public sealed record ReservationPlacedIntegrationEvent(
    Guid ReservationId,
    Guid PerformanceId,
    int Quantity
) : IntegrationEvent;