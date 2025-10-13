using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace EventTickets.Payments.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddPaymentsApplication(
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