// src/EventTickets.Payments.Infrastructure/ServiceCollectionExtensions.cs
using EventTickets.Payments.Application.Abstractions;
using EventTickets.Payments.Application.IntegrationHandlers;
using EventTickets.Payments.Infrastructure.Outbox;
using EventTickets.Payments.Infrastructure.Persistance;
using EventTickets.Payments.Infrastructure.Services;
using EventTickets.Shared;
using EventTickets.Shared.Integration;
using EventTickets.Shared.IntegrationEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventTickets.Payments.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPaymentsModule(
        this IServiceCollection services,
        string? connectionString)
    {
        services.AddDbContext<PaymentsDbContext>(opt =>
            opt.UseNpgsql(connectionString, b => b.MigrationsHistoryTable("__EFMigrationsHistory", SchemaNames.Payments)));

        services.AddScoped<IPaymentIntentRepository, PaymentIntentRepository>();
        services.AddScoped<IPaymentUnitOfWork, PaymentUnitOfWork>();
        services.AddScoped<IPaymentIntentService, PaymentIntentService>();
        services.AddScoped<Outbox.IOutbox, EfOutbox>(); // outbox del BC Payments

        services.AddScoped<IIntegrationEventHandler<ReservationPlacedIntegrationEvent>,
                          ReservationPlacedHandler>();

        return services;
    }
}