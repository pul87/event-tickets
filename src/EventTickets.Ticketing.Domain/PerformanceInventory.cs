using EventTickets.Shared;

namespace EventTickets.Ticketing.Domain;

public sealed class PerformanceInventory : AggregateRoot<Guid>
{
    public int Capacity { get; private set; }
    public int Reserved { get; private set; }
    public int Sold     { get; private set; }
    public uint Version { get; private set; } // concurrency token (Postgres xmin)

    private PerformanceInventory() { } // EF

    public static PerformanceInventory Create(Guid performanceId, int capacity)
    {
        if (capacity <= 0) throw new DomainException("Capacity must be > 0");
        return new PerformanceInventory { Id = performanceId, Capacity = capacity };
    }

    public bool TryReserve(int qty)
    {
        if (qty <= 0) throw new DomainException("Quantity must be > 0");
        if (Reserved + Sold + qty > Capacity) return false;
        Reserved += qty;
        return true;
    }

    public void Confirm(int qty)
    {
        if (qty <= 0 || qty > Reserved) throw new DomainException("Invalid confirm qty");
        Reserved -= qty;
        Sold     += qty;
    }

    public void Release(int qty)
    {
        if (qty <= 0 || qty > Reserved) throw new DomainException("Invalid release qty");
        Reserved -= qty;
    }

    public void Resize(int newCapacity)
    {
        if (newCapacity <= 0) throw new DomainException("Capacity must be > 0");
        if (Reserved + Sold > newCapacity)
            throw new InvalidOperationException("Cannot shrink below Reserved+Sold");
        Capacity = newCapacity;
    }
}
