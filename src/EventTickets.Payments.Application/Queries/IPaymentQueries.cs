namespace EventTickets.Payments.Application.Queries;

public sealed record PaymentIntentionDto(Guid PaymentId, Guid ReservationId, string? PayUrl, string Status);

public interface IPaymentQueries
{
    Task<PaymentIntentionDto?> GetByReservationAsync(Guid reservationId, CancellationToken ct = default);
}