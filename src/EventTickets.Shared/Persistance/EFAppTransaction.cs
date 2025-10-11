using System.Data;
using System.Security.AccessControl;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace EventTickets.Shared.Persistance;

public sealed class EFAppTransaction : IAppTransaction
{
    private readonly NpgsqlConnection _conn;
    private NpgsqlTransaction? _tx;

    public EFAppTransaction(NpgsqlConnection conn) => _conn = conn;
    public async Task BeginAsync(CancellationToken ct = default)
    {
        if (_conn.State != ConnectionState.Open)
            await _conn.OpenAsync();
        _tx = await _conn.BeginTransactionAsync(ct);
    }

    public async Task CommitAsync(CancellationToken ct = default)
    {
        if (_tx is not null) await _tx.CommitAsync(ct); 
    }

    public async ValueTask DisposeAsync()
    {
        if (_tx is not null) await _tx.DisposeAsync();
    }

    public async Task EnlistAsync(DbContext context, CancellationToken ct = default)
    {
        if (_tx is null) throw new InvalidOperationException("Transaction not started");
        await context.Database.UseTransactionAsync(_tx, ct);
    }

    public async Task RollbackAsync(CancellationToken ct = default)
    {
        if (_tx is not null) await _tx.RollbackAsync(ct);
    }
}