using EventTickets.Shared;
using EventTickets.Ticketing.Application.Abstractions;
using EventTickets.Ticketing.Domain;
using MediatR;

namespace EventTickets.Ticketing.Application.Reservations;

public sealed record ConfirmReservation(Guid ReservationId) : IRequest<Unit>;

public sealed class ConfirmReservationHandler : IRequestHandler<ConfirmReservation, Unit>
{
    private readonly IPerformanceInventoryRepository _inventory;
    private readonly IReservationRepository _reservations;
    private readonly ITicketingUnitOfWork _uow;

    public ConfirmReservationHandler(IPerformanceInventoryRepository inventory,
                                     IReservationRepository reservations,
                                     ITicketingUnitOfWork uow)
        => (_inventory, _reservations, _uow) = (inventory, reservations, uow);

    public async Task<Unit> Handle(ConfirmReservation cmd, CancellationToken ct)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                var res = await _reservations.GetByIdAsync(cmd.ReservationId, ct)
                          ?? throw new NotFoundException("Reservation not found");

                if (res.Status == ReservationStatus.Confirmed) return Unit.Value;
                if (res.Status != ReservationStatus.PendingPayment)
                    throw new DomainException("Reservation is not pending");

                var inv = await _inventory.GetByIdAsync(res.PerformanceId, ct)
                          ?? throw new NotFoundException("Performance inventory not found");

                inv.Confirm(res.Quantity);
                res.Confirm();

                await _uow.SaveChangesAsync(ct);
                return Unit.Value;
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
