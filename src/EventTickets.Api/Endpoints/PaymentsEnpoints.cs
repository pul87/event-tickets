using EventTickets.Payments.Application.PaymentIntents;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EventTickets.Api.Endpoints;

public static class PaymentsEndpoints
{
    public static RouteGroupBuilder Map(this RouteGroupBuilder group)
    {
        group.MapGet("/payments/intents/{reservationId}",
            async Task<Results<Ok<PaymentIntentDto>, NotFound>> (Guid reservationId, IMediator mediator, CancellationToken ct) =>
            {
                var dto = await mediator.Send(new GetPaymentIntentByReservation(reservationId), ct);
                
                if (dto is null)
                    return TypedResults.NotFound();
                    
                return TypedResults.Ok(dto);
            })
            .Produces<PaymentIntentDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi();
            
        group.MapPost("/payments/webhooks",
            async Task<Results<Ok<ProcessWebhookResult>, BadRequest<string>, NotFound>> (
                ProcessWebhook request, 
                IMediator mediator, 
                CancellationToken ct) =>
            {
                try
                {
                    var result = await mediator.Send(request, ct);
                    
                    if (result is null)
                        return TypedResults.NotFound();
                        
                    return TypedResults.Ok(result);
                }
                catch (Exception ex)
                {
                    return TypedResults.BadRequest(ex.Message);
                }
            })
            .Produces<ProcessWebhookResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi();
        return group;
    }
}

