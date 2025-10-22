using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using EventTickets.Ticketing.Application.Reservations;

namespace EventTickets.Api.Endpoints;

public static class SalesEndpoints
{
    public static RouteGroupBuilder Map(this RouteGroupBuilder group)
    {
        group.MapPost("/reservations",
            async (PlaceReservationRequest  body, IMediator mediator, CancellationToken ct) =>
            {
                var r = await mediator.Send(new PlaceReservation(body.PerformanceId, body.Quantity), ct);
                return TypedResults.Created($"/sales/reservations/{r.ReservationId}", r);
            })
            .Produces<ReservationPlaced>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .WithOpenApi();

        group.MapPost("/reservations/{id:guid}/confirm",
            async (Guid id, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new ConfirmReservation(id), ct);
                return TypedResults.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .WithOpenApi();

        group.MapPost("/reservations/{id:guid}/cancel",
            async (Guid id, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new CancelReservation(id), ct);
                return TypedResults.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .WithOpenApi();

        return group;
    }
}

public sealed record PlaceReservationRequest(Guid PerformanceId, int Quantity);
