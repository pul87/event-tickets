using EventTickets.Payments.Application.Abstractions;
using EventTickets.Payments.Application.Integration;
using EventTickets.Payments.Domain;
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
}