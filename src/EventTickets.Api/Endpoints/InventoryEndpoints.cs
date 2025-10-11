using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using EventTickets.Ticketing.Application.Inventory;

namespace EventTickets.Api.Endpoints;

public static class InventoryEndpoints
{
    public static RouteGroupBuilder Map(this RouteGroupBuilder group)
    {
        // POST /inventory/performances → 201
        group.MapPost("/performances",
            async (CreateInventoryRequest body, IMediator mediator, CancellationToken ct) =>
            {
                var created = await mediator.Send(new CreatePerformanceInventory(body.PerformanceId, body.Capacity), ct);
                return TypedResults.Created($"/inventory/performances/{created.PerformanceId}", new { id = created.PerformanceId });
            })
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .WithOpenApi();

        // PUT /inventory/performances/{id}/capacity → 204
        group.MapPut("/performances/{performanceId:guid}/capacity",
            async (Guid performanceId, ResizeCapacityRequest body, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new ResizePerformanceCapacity(performanceId, body.NewCapacity), ct);
                return TypedResults.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .WithOpenApi();

        // GET /inventory/performances/{id} → 200/404
        group.MapGet("/performances/{id:guid}",
            async Task<Results<Ok<PerformanceInventoryDto>, NotFound>> (Guid id, IMediator mediator, CancellationToken ct) =>
            {
                var dto = await mediator.Send(new GetPerformanceInventory(id), ct);
                return TypedResults.Ok(dto);
            })
            .Produces<PerformanceInventoryDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi();

        return group;
    }

    // DTO di API (contratti HTTP), separati dai Command
    public sealed record CreateInventoryRequest(Guid PerformanceId, int Capacity);
    public sealed record ResizeCapacityRequest(int NewCapacity);
}
