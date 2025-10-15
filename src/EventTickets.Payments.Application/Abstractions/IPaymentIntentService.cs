using EventTickets.Payments.Application.PaymentIntents;

namespace EventTickets.Payments.Application.Abstractions;

public interface IPaymentIntentService
{
    Task CreateForReservationAsync(Guid reservationId, int quantity, CancellationToken ct);
    Task<ProcessWebhookResult> ProcessWebhookAsync(ProcessWebhook payload, CancellationToken ct);
}