using EventTickets.Ticketing.Domain;

namespace EventTickets.Ticketing.Application.Abstractions;

public interface IPerformanceInventoryRepository
{
    Task<PerformanceInventory?> GetByIdAsync(Guid id, CancellationToken ct);
    void Add(PerformanceInventory entity);
}
