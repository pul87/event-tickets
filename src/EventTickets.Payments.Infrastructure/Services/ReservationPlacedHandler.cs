using EventTickets.Shared.Integration;
using EventTickets.Shared.IntegrationEvents;

namespace EventTickets.Payments.Infrastructure.Services;

public sealed class ReservationPlacedHandler : IIntegrationEventHandler<ReservationPlacedIntegrationEvent>
{
    private readonly IPaymentIntentService _paymentService;

    public ReservationPlacedHandler(IPaymentIntentService paymentService)
        => _paymentService = paymentService;

    public async Task HandleAsync(ReservationPlacedIntegrationEvent @event, CancellationToken ct)
    {
        // Quando viene creata una prenotazione, creiamo automaticamente un PaymentIntent
        await _paymentService.CreateForReservationAsync(@event.ReservationId, @event.Quantity, ct);
    }
}