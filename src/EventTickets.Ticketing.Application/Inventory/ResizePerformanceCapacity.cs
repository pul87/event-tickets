using EventTickets.Shared;
using EventTickets.Ticketing.Application.Abstractions;
using MediatR;

namespace EventTickets.Ticketing.Application.Inventory;

public sealed record ResizePerformanceCapacity(Guid PerformanceId, int NewCapacity) : IRequest<Unit>;

public sealed class ResizePerformanceCapacityHandler
    : IRequestHandler<ResizePerformanceCapacity, Unit>
{
    private readonly IPerformanceInventoryRepository _repo;
    private readonly ITicketingUnitOfWork _uow;

    public ResizePerformanceCapacityHandler(IPerformanceInventoryRepository repo, ITicketingUnitOfWork uow)
        => (_repo, _uow) = (repo, uow);

    public async Task<Unit> Handle(ResizePerformanceCapacity cmd, CancellationToken ct)
    {
        if (cmd.NewCapacity <= 0) throw new DomainException("Capacity must be > 0");

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                var pi = await _repo.GetByIdAsync(cmd.PerformanceId, ct)
                         ?? throw new NotFoundException("PerformanceInventory not found");

                pi.Resize(cmd.NewCapacity);
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
