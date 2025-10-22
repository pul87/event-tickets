using EventTickets.Shared.IntegrationEvents;

namespace EventTickets.Shared.IntegrationEvents;

public sealed record PaymentResultIntegrationEvent (
    Guid PaymentId,
    Guid ReservationId,
    bool Success
) : IntegrationEvent;