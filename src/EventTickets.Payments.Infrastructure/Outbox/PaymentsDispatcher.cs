using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using EventTickets.Shared.Outbox;

namespace EventTickets.Payments.Infrastructure.Outbox;

public sealed class PaymentsOutboxDispatcher : OutboxDispatcher<PaymentsDbContext>
{
    public PaymentsOutboxDispatcher(
        IServiceScopeFactory scopeFactory, 
        ILogger<PaymentsOutboxDispatcher> logger) 
        : base(scopeFactory, logger)
    {
    }

    protected override async Task<List<OutboxMessage>> GetOutboxMessagesAsync(
        PaymentsDbContext db, 
        int take, 
        CancellationToken ct)
    {
        return await db.OutboxMessages
            .Where(x => x.ProcessedOnUtc == null)
            .OrderBy(x => x.OccurredOnUtc)
            .Take(take)
            .ToListAsync(ct);
    }

    protected override Task MarkAsProcessedAsync(PaymentsDbContext db, OutboxMessage msg)
    {
        msg.ProcessedOnUtc = DateTime.UtcNow;
        return Task.CompletedTask;
    }

    protected override Task MarkAsErrorAsync(PaymentsDbContext db, OutboxMessage msg, string error)
    {
        msg.Error = error;
        return Task.CompletedTask;
    }

    protected override Task SaveChangesAsync(PaymentsDbContext db, CancellationToken ct)
    {
        return db.SaveChangesAsync(ct);
    }
}