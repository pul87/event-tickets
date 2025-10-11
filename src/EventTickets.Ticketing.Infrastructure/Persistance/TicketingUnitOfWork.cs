using EventTickets.Shared;
using EventTickets.Ticketing.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace EventTickets.Ticketing.Infrastructure;

public sealed class TicketingUnitOfWork : ITicketingUnitOfWork
{
    private readonly TicketingDbContext _db;
    public TicketingUnitOfWork(TicketingDbContext db) => _db = db;

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        try
        {
            return await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyException(inner: ex);
        }
    }

    public void Clear() => _db.ChangeTracker.Clear();
}
