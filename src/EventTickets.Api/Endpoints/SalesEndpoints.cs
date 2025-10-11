using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using EventTickets.Ticketing.Application.Reservations;

namespace EventTickets.Api.Endpoints;

public static class SalesEndpoints
{
    public static RouteGroupBuilder Map(this RouteGroupBuilder group)
    {
        // POST /sales/reservations → 201
        group.MapPost("/reservations",
            async (PlaceReservation cmd, IMediator mediator, CancellationToken ct) =>
            {
                var r = await mediator.Send(cmd, ct);
                return TypedResults.Created($"/sales/reservations/{r.ReservationId}", r);
            })
            .Produces<ReservationPlaced>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .WithOpenApi();

        // POST /sales/reservations/{id}/confirm → 204
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

        // POST /sales/reservations/{id}/cancel → 204
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
