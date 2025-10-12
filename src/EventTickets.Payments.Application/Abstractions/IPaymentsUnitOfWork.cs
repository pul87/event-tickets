namespace EventTickets.Payments.Application.Abstractions;

public interface IPaymentUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    void Clear();
}