using EventTickets.Shared.IntegrationEvents;

namespace EventTickets.Shared.IntegrationEvents;

public sealed record PaymentRequestedIntegrationEvent(
    Guid ReservationId,
    Guid PaymentId,
    string PayUrl
) : IntegrationEvent;