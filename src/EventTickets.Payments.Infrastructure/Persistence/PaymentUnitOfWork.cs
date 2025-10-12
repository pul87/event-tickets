using EventTickets.Payments.Application.Abstractions;

namespace EventTickets.Payments.Infrastructure.Persistance;

public sealed class PaymentUnitOfWork : IPaymentUnitOfWork
{
    private readonly PaymentsDbContext _db;
    public PaymentUnitOfWork(PaymentsDbContext db) => _db = db;

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);

    public void Clear() => _db.ChangeTracker.Clear();
}