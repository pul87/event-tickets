using EventTickets.Ticketing.Application;
using EventTickets.Ticketing.Infrastructure;
using EventTickets.Api.Endpoints;
using Microsoft.AspNetCore.OpenApi;
using EventTickets.Payments.Infrastructure;
using EventTickets.Payments.Application;

var builder = WebApplication.CreateBuilder(args);

// Infrastructure: DbContext + Repo + UoW
var conn = builder.Configuration.GetConnectionString("Default");
if (string.IsNullOrWhiteSpace(conn))
    throw new InvalidOperationException(@"
        Missing DB connection string 'ConnectionStrings:Default'.
        Add it to apsettings.json or set it via env var.
    ");

builder.Services.AddTicketingModule(conn);
builder.Services.AddPaymentsModule(conn);

// Mediatr license key
var mediatrKey =
    builder.Configuration["MediatR:LicenseKey"] ??
    // in caso di typo
    builder.Configuration["Mediatr:LicenseKey"];

// Application: registra tutti gli handler MediatR dall'assembly Application
builder.Services.AddTicketingApplication(mediatrKey);
builder.Services.AddPaymentsApplication(mediatrKey);

// API goodies
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Middleware: mapping eccezioni → HTTP (già nel progetto)
builder.Services.AddTransient<ExceptionMappingMiddleware>();

var app = builder.Build();

app.UseMiddleware<ExceptionMappingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Environment.IsProduction() && string.IsNullOrWhiteSpace(mediatrKey))
{
    app.Logger.LogWarning(
        @"MediatR lincese key is missing in Production.
        Set MEDIATR__LICENSEKEY or configuration 'MediatR:LicenseKey'
        ");
}

app.MapGet("/health", () => new { ok = true, ts = DateTime.UtcNow })
   .WithTags("System")
   .WithOpenApi();

// Endpoint groups (ordinati per feature)
var inventory = app.MapGroup("/inventory").WithTags("Inventory");
InventoryEndpoints.Map(inventory);               // NEW

var sales = app.MapGroup("/sales").WithTags("Sales");
SalesEndpoints.Map(sales);                       // NEW

var payments = app.MapGroup("/payments").WithTags("Payments");
PaymentsEndpoints.Map(payments);

app.Run();
