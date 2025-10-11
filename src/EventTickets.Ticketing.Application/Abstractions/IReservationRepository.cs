using EventTickets.Ticketing.Domain;

namespace EventTickets.Ticketing.Application.Abstractions;

public interface IReservationRepository
{
    Task<Reservation?> GetByIdAsync(Guid id, CancellationToken ct);
    void Add(Reservation entity);
}
