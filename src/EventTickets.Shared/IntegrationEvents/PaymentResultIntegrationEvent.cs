using EventTickets.Shared.Integration;

namespace EventTickets.Shared.IntegrationEvents;

public sealed record PaymentResultIntegrationEvent (
    Guid PaymentId,
    Guid ReservationId,
    bool Success
) : IntegrationEvent;