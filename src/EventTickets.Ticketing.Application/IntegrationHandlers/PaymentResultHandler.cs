using EventTickets.Shared;
using EventTickets.Shared.IntegrationEvents;
using EventTickets.Ticketing.Application.Abstractions;

namespace EventTickets.Ticketing.Application.IntegrationHandlers;

public sealed class PaymentResultHandler : IIntegrationEventHandler<PaymentResultIntegrationEvent>
{

    private readonly IReservationRepository _reservationRepo;
    private readonly IPerformanceInventoryRepository _performanceRepo;
    private readonly ITicketingUnitOfWork _uow;

    public PaymentResultHandler(
        IReservationRepository reservationRepo,
        IPerformanceInventoryRepository performanceRepo,
        ITicketingUnitOfWork uow
        ) => (_reservationRepo, _performanceRepo, _uow) = (reservationRepo, performanceRepo, uow);
    public async Task HandleAsync(PaymentResultIntegrationEvent @event, CancellationToken ct)
    {
        var reservation = await _reservationRepo.GetByIdAsync(@event.ReservationId, ct);

        if (reservation is null)
            throw new NotFoundException($"Reservation with id {@event.ReservationId} not found");

        var performance = await _performanceRepo.GetByIdAsync(reservation.PerformanceId, ct);

        if (performance is null)
            throw new NotFoundException($"Performance with id {reservation.PerformanceId}");

        if (@event.Success) 
        {
            reservation.Confirm();
            performance.Confirm(reservation.Quantity);
        }
        else
        {
            reservation.Cancel();
            performance.Release(reservation.Quantity);
        }
        await _uow.SaveChangesAsync(ct);
    }
}