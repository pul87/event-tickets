using EventTickets.Payments.Application.Queries;
using Microsoft.EntityFrameworkCore;

namespace EventTickets.Payments.Infrastructure.Queries;

public sealed class PaymentQueries : IPaymentQueries
{
    private PaymentsDbContext _db;
    public PaymentQueries(PaymentsDbContext db) => _db = db;
    public async Task<PaymentIntentionDto?> GetByReservationAsync(Guid reservationId, CancellationToken ct = default)
     => await _db.PaymentIntents
            .Where(x => x.ReservationId == reservationId)
            .Select(x => new PaymentIntentionDto(x.Id, x.ReservationId, x.PayUrl, x.Status.ToString()))
            .SingleOrDefaultAsync();
}