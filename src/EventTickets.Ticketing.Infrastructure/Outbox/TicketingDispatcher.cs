using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using EventTickets.Shared.Outbox;

namespace EventTickets.Ticketing.Infrastructure.Outbox;

public sealed class TicketingOutboxDispatcher : OutboxDispatcher<TicketingDbContext>
{
    public TicketingOutboxDispatcher(
        IServiceScopeFactory scopeFactory, 
        ILogger<TicketingOutboxDispatcher> logger) 
        : base(scopeFactory, logger)
    {
    }

    protected override async Task<List<OutboxMessage>> GetOutboxMessagesAsync(
        TicketingDbContext db, 
        int take, 
        CancellationToken ct)
    {
        return await db.OutboxMessages
            .Where(x => x.ProcessedOnUtc == null)
            .OrderBy(x => x.OccurredOnUtc)
            .Take(take)
            .ToListAsync(ct);
    }

    protected override Task MarkAsProcessedAsync(TicketingDbContext db, OutboxMessage msg)
    {
        msg.ProcessedOnUtc = DateTime.UtcNow;
        return Task.CompletedTask;
    }

    protected override Task MarkAsErrorAsync(TicketingDbContext db, OutboxMessage msg, string error)
    {
        msg.Error = error;
        return Task.CompletedTask;
    }

    protected override Task SaveChangesAsync(TicketingDbContext db, CancellationToken ct)
    {
        return db.SaveChangesAsync(ct);
    }
}