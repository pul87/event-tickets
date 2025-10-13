using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using EventTickets.Ticketing.Application.Abstractions;
using EventTickets.Shared.Outbox;
using EventTickets.Ticketing.Infrastructure.Outbox;

namespace EventTickets.Ticketing.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTicketingModule(
        this IServiceCollection services,
        string? connectionString)
    {
        services.AddDbContext<TicketingDbContext>(opt => opt.UseNpgsql(connectionString));
        services.AddScoped<ITicketingUnitOfWork, TicketingUnitOfWork>();
        services.AddScoped<IPerformanceInventoryRepository, PerformanceInventoryRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddScoped<IOutbox, EfOutbox>();

        services.AddHostedService<TicketingOutboxDispatcher>();
        return services;
    }
}
