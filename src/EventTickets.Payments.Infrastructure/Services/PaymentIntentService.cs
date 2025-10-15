using System.Runtime.CompilerServices;
using EventTickets.Payments.Application.Abstractions;
using EventTickets.Payments.Application.PaymentIntents;
using EventTickets.Payments.Domain;
using EventTickets.Shared;
using EventTickets.Shared.IntegrationEvents;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace EventTickets.Payments.Infrastructure.Services;

public sealed class PaymentIntentService : IPaymentIntentService
{
    private readonly IPaymentIntentRepository _repo;
    private readonly IPaymentUnitOfWork _uow;
    private readonly Outbox.IOutbox _outbox;

    public PaymentIntentService(IPaymentIntentRepository repo, IPaymentUnitOfWork uow, Outbox.IOutbox outbox)
        => (_repo, _uow, _outbox) = (repo, uow, outbox);

    public async Task CreateForReservationAsync(Guid reservationId, int quantity, CancellationToken ct)
    {
        var existing = await _repo.GetByReservationAsync(reservationId, ct);

        if (existing is not null) return;

        var amount = quantity * 10m;
        var payUrl = $"https://pay.local/checkout?reservationId={reservationId}";
        var intent = PaymentIntent.Create(reservationId, amount, payUrl);

        _repo.Add(intent);

        await _outbox.EnqueueAsync(new PaymentRequestedIntegrationEvent(reservationId, intent.Id, intent.PayUrl!), ct);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<ProcessWebhookResult> ProcessWebhookAsync(ProcessWebhook payload, CancellationToken ct)
    {
        var pi = await _repo.GetByIdAsync(payload.PaymentIntentId, ct);

        if (pi is null) 
            throw new NotFoundException($"Payment Id {payload.PaymentIntentId} not found.");

        var success = ProcessPaymentEvent(pi, payload.EventType);

        await _outbox.EnqueueAsync(
            new PaymentResultIntegrationEvent(
                pi.Id, pi.ReservationId, success), ct);

        await _uow.SaveChangesAsync(ct);

        return new ProcessWebhookResult(success, payload.FailureReason, pi.Status);
    }


    private static bool ProcessPaymentEvent(PaymentIntent pi, WebhookEventType eventType)
    {
        switch (eventType) {
            case WebhookEventType.PaymentSucceeded:
                pi.Capture();
                return true;
            case WebhookEventType.PaymentFailed:
                pi.Fail();
                return false;
            case WebhookEventType.PaymentCancelled:
                pi.Cancel();
                return false;
            default:
                throw new InvalidOperationException($"Invalid event type {eventType}");
        }
    }
}

