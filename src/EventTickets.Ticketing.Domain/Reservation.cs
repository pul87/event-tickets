using EventTickets.Shared;

namespace EventTickets.Ticketing.Domain;

public enum ReservationStatus { PendingPayment, Confirmed, Cancelled, Expired }

public sealed class Reservation : AggregateRoot<Guid>
{
    public Guid PerformanceId { get; private set; }
    public int Quantity { get; private set; }
    public ReservationStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Reservation() { }

    public static Reservation Place(Guid performanceId, int quantity)
    {
        if (quantity <= 0) throw new DomainException("Quantity must be > 0");
        return new Reservation
        {
            Id = Guid.NewGuid(),
            PerformanceId = performanceId,
            Quantity = quantity,
            Status = ReservationStatus.PendingPayment,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Confirm()
    {
        if (Status == ReservationStatus.Confirmed) return;
        if (Status != ReservationStatus.PendingPayment) throw new DomainException("Not pending");
        Status = ReservationStatus.Confirmed;
    }

    public void Cancel()
    {
        if (Status == ReservationStatus.Cancelled) return;
        if (Status != ReservationStatus.PendingPayment) throw new DomainException("Not pending");
        Status = ReservationStatus.Cancelled;
    }
}
