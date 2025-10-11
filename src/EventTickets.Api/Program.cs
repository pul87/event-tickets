using EventTickets.Ticketing.Application;          // estensione AddTicketingApplication (nuova)
using EventTickets.Ticketing.Infrastructure;       // AddTicketingModule (già presente)
using EventTickets.Api.Endpoints;                  // classi statiche di mapping endpoints
using Microsoft.AspNetCore.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Infrastructure: DbContext + Repo + UoW (EF sta qui, non in API)
builder.Services.AddTicketingModule(builder.Configuration.GetConnectionString("Default"));

// Application: registra tutti gli handler MediatR dall'assembly Application
builder.Services.AddTicketingApplication();

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

app.MapGet("/health", () => new { ok = true, ts = DateTime.UtcNow })
   .WithTags("System")
   .WithOpenApi();

// Endpoint groups (ordinati per feature)
var inventory = app.MapGroup("/inventory").WithTags("Inventory");
InventoryEndpoints.Map(inventory);               // NEW

var sales = app.MapGroup("/sales").WithTags("Sales");
SalesEndpoints.Map(sales);                       // NEW

app.Run();
