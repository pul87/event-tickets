using EventTickets.Shared;
using EventTickets.Ticketing.Application.Abstractions;
using EventTickets.Ticketing.Domain;
using MediatR;

namespace EventTickets.Ticketing.Application.Inventory;

public sealed record CreatePerformanceInventory(Guid PerformanceId, int Capacity)
    : IRequest<CreatedPerformanceInventory>;

public sealed record CreatedPerformanceInventory(Guid PerformanceId);

public sealed class CreatePerformanceInventoryHandler
    : IRequestHandler<CreatePerformanceInventory, CreatedPerformanceInventory>
{
    private readonly IPerformanceInventoryRepository _repo;
    private readonly ITicketingUnitOfWork _uow;

    public CreatePerformanceInventoryHandler(IPerformanceInventoryRepository repo, ITicketingUnitOfWork uow)
        => (_repo, _uow) = (repo, uow);

    public async Task<CreatedPerformanceInventory> Handle(CreatePerformanceInventory cmd, CancellationToken ct)
    {
        if (cmd.Capacity <= 0) throw new DomainException("Capacity must be > 0");

        var existing = await _repo.GetByIdAsync(cmd.PerformanceId, ct);
        if (existing is not null)
            throw new InvalidOperationException("PerformanceInventory already exists");

        _repo.Add(PerformanceInventory.Create(cmd.PerformanceId, cmd.Capacity));
        await _uow.SaveChangesAsync(ct);

        return new CreatedPerformanceInventory(cmd.PerformanceId);
    }
}
