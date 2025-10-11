using EventTickets.Ticketing.Application.Abstractions;
using EventTickets.Ticketing.Domain;
using Microsoft.EntityFrameworkCore;

namespace EventTickets.Ticketing.Infrastructure;

public sealed class PerformanceInventoryRepository : IPerformanceInventoryRepository
{
    private readonly TicketingDbContext _db;
    public PerformanceInventoryRepository(TicketingDbContext db) => _db = db;

    public Task<PerformanceInventory?> GetByIdAsync(Guid id, CancellationToken ct) =>
        _db.PerformanceInventories.FirstOrDefaultAsync(x => x.Id == id, ct);

    public void Add(PerformanceInventory entity) => _db.PerformanceInventories.Add(entity);
}
