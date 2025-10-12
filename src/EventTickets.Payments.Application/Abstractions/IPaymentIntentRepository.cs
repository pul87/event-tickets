using EventTickets.Payments.Domain;

namespace EventTickets.Payments.Application.Abstractions;

public interface IPaymentIntentRepository
{
    Task<PaymentIntent?> GetByReservationAsync(Guid reservationId, CancellationToken ct);
    Task<PaymentIntent?> GetByIdAsync(Guid id, CancellationToken ct);
    void Add(PaymentIntent intent);
}