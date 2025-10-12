using EventTickets.Shared.Integration;

namespace EventTickets.Payments.Application.Integration;

public sealed record PaymentRequestedIntegrationEvent(
    Guid ReservationId,
    Guid PaymentId,
    string PayUrl
) : IntegrationEvent;