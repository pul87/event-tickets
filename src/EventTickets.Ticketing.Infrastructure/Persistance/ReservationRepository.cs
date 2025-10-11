using EventTickets.Ticketing.Application.Abstractions;
using EventTickets.Ticketing.Domain;
using Microsoft.EntityFrameworkCore;

namespace EventTickets.Ticketing.Infrastructure;

public sealed class ReservationRepository : IReservationRepository
{
    private readonly TicketingDbContext _db;
    public ReservationRepository(TicketingDbContext db) => _db = db;

    public Task<Reservation?> GetByIdAsync(Guid id, CancellationToken ct) =>
        _db.Reservations.FirstOrDefaultAsync(x => x.Id == id, ct);

    public void Add(Reservation entity) => _db.Reservations.Add(entity);
}
