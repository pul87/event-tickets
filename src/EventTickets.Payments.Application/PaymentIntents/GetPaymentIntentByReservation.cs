using EventTickets.Shared;
using EventTickets.Payments.Application.Abstractions;
using MediatR;

namespace EventTickets.Payments.Application.PaymentIntents;

public sealed record GetPaymentIntentByReservation(Guid ReservationId)
    : IRequest<PaymentIntentDto?>;

public sealed record PaymentIntentDto(
    Guid Id, 
    Guid ReservationId, 
    decimal Amount, 
    string? PayUrl, 
    string Status
);

public sealed class GetPaymentIntentByReservationHandler
    : IRequestHandler<GetPaymentIntentByReservation, PaymentIntentDto?>
{
    private readonly IPaymentIntentRepository _repo;

    public GetPaymentIntentByReservationHandler(IPaymentIntentRepository repo) => _repo = repo;

    public async Task<PaymentIntentDto?> Handle(GetPaymentIntentByReservation query, CancellationToken ct)
    {
        var entity = await _repo.GetByReservationAsync(query.ReservationId, ct);
        
        if (entity is null) return null;

        // Domain Entity → DTO
        return new PaymentIntentDto(
            entity.Id, 
            entity.ReservationId, 
            entity.Amount, 
            entity.PayUrl, 
            entity.Status.ToString()
        );
    }
}