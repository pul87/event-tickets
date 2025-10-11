using EventTickets.Shared;
using EventTickets.Ticketing.Application.Abstractions;
using MediatR;

namespace EventTickets.Ticketing.Application.Inventory;

public sealed record GetPerformanceInventory(Guid PerformanceId)
    : IRequest<PerformanceInventoryDto>;

public sealed record PerformanceInventoryDto(Guid Id, int Capacity, int Reserved, int Sold);

public sealed class GetPerformanceInventoryHandler
    : IRequestHandler<GetPerformanceInventory, PerformanceInventoryDto>
{
    private readonly IPerformanceInventoryRepository _repo;

    public GetPerformanceInventoryHandler(IPerformanceInventoryRepository repo) => _repo = repo;

    public async Task<PerformanceInventoryDto> Handle(GetPerformanceInventory q, CancellationToken ct)
    {
        var pi = await _repo.GetByIdAsync(q.PerformanceId, ct)
                 ?? throw new NotFoundException("PerformanceInventory not found");

        return new PerformanceInventoryDto(pi.Id, pi.Capacity, pi.Reserved, pi.Sold);
    }
}
