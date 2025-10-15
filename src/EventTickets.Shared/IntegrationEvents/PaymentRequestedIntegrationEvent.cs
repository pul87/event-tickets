using EventTickets.Shared.Integration;

namespace EventTickets.Shared.IntegrationEvents;

public sealed record PaymentRequestedIntegrationEvent(
    Guid ReservationId,
    Guid PaymentId,
    string PayUrl
) : IntegrationEvent;