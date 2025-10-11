using System.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace EventTickets.Ticketing.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddTicketingApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        return services;
    }
}
