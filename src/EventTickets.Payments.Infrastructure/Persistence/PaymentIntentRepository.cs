using EventTickets.Payments.Application.Abstractions;
using EventTickets.Payments.Domain;
using Microsoft.EntityFrameworkCore;

namespace EventTickets.Payments.Infrastructure.Persistance;

public sealed class PaymentIntentRepository : IPaymentIntentRepository
{
    private readonly PaymentsDbContext _db;

    public PaymentIntentRepository(PaymentsDbContext db) => _db = db;

    public void Add(PaymentIntent intent) => _db.PaymentIntents.Add(intent);

    public Task<PaymentIntent?> GetByIdAsync(Guid id, CancellationToken ct)
        =>_db.PaymentIntents.SingleOrDefaultAsync(x => x.Id == id, ct);

    public Task<PaymentIntent?> GetByReservationAsync(Guid reservationId, CancellationToken ct) =>
        _db.PaymentIntents.SingleOrDefaultAsync(x => x.ReservationId == reservationId, ct);
    
}