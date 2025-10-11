namespace EventTickets.Ticketing.Application.Abstractions;

public interface ITicketingUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    void Clear(); // pulizia per retry dopo conflitto
}
