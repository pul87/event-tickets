namespace EventTickets.Payments.Application.Abstractions;

public interface IPaymentIntentService
{
    Task CreateForReservationAsync(Guid reservationId, int quantity, CancellationToken ct);
}