

using Microsoft.EntityFrameworkCore;

namespace EventTickets.Shared.Persistance;

public interface IAppTransaction : IAsyncDisposable
{
    Task BeginAsync(CancellationToken ct = default);
    Task EnlistAsync(DbContext context, CancellationToken ct = default);
    Task CommitAsync(CancellationToken ct = default);
    Task RollbackAsync(CancellationToken ct = default);
}