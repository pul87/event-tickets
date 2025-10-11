using EventTickets.Shared;
using EventTickets.Ticketing.Application.Abstractions;
using EventTickets.Ticketing.Domain;
using MediatR;

namespace EventTickets.Ticketing.Application.Reservations;

public sealed record PlaceReservation(Guid PerformanceId, int Quantity)
    : IRequest<ReservationPlaced>;

public sealed record ReservationPlaced(Guid ReservationId, Guid PerformanceId, int Quantity);

public sealed class PlaceReservationHandler
    : IRequestHandler<PlaceReservation, ReservationPlaced>
{
    private readonly IPerformanceInventoryRepository _inventory;
    private readonly IReservationRepository _reservations;
    private readonly ITicketingUnitOfWork _uow;

    public PlaceReservationHandler(
        IPerformanceInventoryRepository inventory,
        IReservationRepository reservations,
        ITicketingUnitOfWork uow)
        => (_inventory, _reservations, _uow) = (inventory, reservations, uow);

    public async Task<ReservationPlaced> Handle(PlaceReservation cmd, CancellationToken ct)
    {
        if (cmd.Quantity <= 0) throw new DomainException("Quantity must be > 0");

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                var inv = await _inventory.GetByIdAsync(cmd.PerformanceId, ct)
                          ?? throw new NotFoundException("Performance inventory not found");

                if (!inv.TryReserve(cmd.Quantity))
                    throw new InvalidOperationException("Insufficient capacity");

                var res = Reservation.Place(cmd.PerformanceId, cmd.Quantity);
                _reservations.Add(res);

                await _uow.SaveChangesAsync(ct);
                return new ReservationPlaced(res.Id, res.PerformanceId, res.Quantity);
            }
            catch (ConcurrencyException)
            {
                if (attempt >= 3) throw;
                _uow.Clear();
                continue;
            }
        }
    }
}
