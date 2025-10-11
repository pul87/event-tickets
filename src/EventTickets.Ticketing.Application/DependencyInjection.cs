using System.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace EventTickets.Ticketing.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddTicketingApplication(
        this IServiceCollection services,
        string? mediatrLicenseKey = null
        )
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            if (!string.IsNullOrWhiteSpace(mediatrLicenseKey))
                cfg.LicenseKey = mediatrLicenseKey;
        });
        return services;
    }
}
