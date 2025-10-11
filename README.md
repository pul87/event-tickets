EventTickets (Ticketing)





A modular monolith implemented with DDD (Domain/Application/Infrastructure), CQRS with MediatR, and EF Core on PostgreSQL.Single Bounded Context: Ticketing (aggregates: PerformanceInventory, Reservation) under one DB schema: ticketing.

Table of contents

Architecture

Project structure

Getting started

Run

Database & migrations

API

Quality & conventions

Troubleshooting

License

Architecture

Modular Monolith with a single BC: Ticketing

DDD layers

Domain: aggregates, invariants, value objects, domain exceptions

Application: commands/queries + handlers (MediatR), ports (repos/UoW)

Infrastructure: EF Core TicketingDbContext, repositories, UoW, DI extension

API: minimal APIs only call IMediator; no EF usage here

Persistence: PostgreSQL, schema ticketing, optimistic concurrency via Postgres xmin (IsRowVersion())

Project structure

src/
  EventTickets.Api/                     # Web API (composition root)
    Endpoints/
      InventoryEndpoints.cs
      SalesEndpoints.cs
    ExceptionMappingMiddleware.cs
  EventTickets.Ticketing.Application/   # Use cases (MediatR), ports
    Abstractions/
    Inventory/ (Create/Resize/Get)
    Reservations/ (Place/Confirm/Cancel)
    DependencyInjection.cs              # AddTicketingApplication()
  EventTickets.Ticketing.Domain/        # Aggregates + rules
    PerformanceInventory.cs
    Reservation.cs
  EventTickets.Ticketing.Infrastructure/# EF Core & DI extension
    TicketingDbContext.cs
    Persistence/
      PerformanceInventoryRepository.cs
      ReservationRepository.cs
      TicketingUnitOfWork.cs
    ServiceCollectionExtensions.cs      # AddTicketingModule()
  EventTickets.Shared/                  # Cross-cutting exceptions/types
    DomainException.cs, NotFoundException.cs, ConcurrencyException.cs

Getting started

Prerequisites

.NET 8.0 SDK

Docker (for PostgreSQL)

PowerShell or Bash

Configure database

Set the connection string in src/EventTickets.Api/appsettings.json:

{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=eventtickets;Username=et;Password=etpwd"
  }
}

Bring up Postgres (example docker-compose.yml):

services:
  postgres:
    image: postgres:16-alpine
    environment:
      POSTGRES_USER: et
      POSTGRES_PASSWORD: etpwd
      POSTGRES_DB: eventtickets
    ports:
      - "5432:5432"
    volumes:
      - pgdata:/var/lib/postgresql/data
volumes:
  pgdata:

docker compose up -d

Run

dotnet build
dotnet run --project src/EventTickets.Api
# Swagger UI → http://localhost:5000/swagger
# Health     → http://localhost:5000/health

If your ASP.NET profile binds to a different port, adjust Postman/Swagger accordingly.

Database & migrations

Migrations live in Infrastructure.

Create a migration:

dotnet ef migrations add InitialTicketing \
  -p src/EventTickets.Ticketing.Infrastructure \
  -s src/EventTickets.Api \
  -o Migrations

Apply:

dotnet ef database update \
  -p src/EventTickets.Ticketing.Infrastructure \
  -s src/EventTickets.Api

Tip: If you prefer design-time independence, add an IDesignTimeDbContextFactory<TicketingDbContext> in Infrastructure.

API

Inventory

POST /inventory/performances → create capacityBody:

{ "performanceId": "GUID", "capacity": 5 }

201 Created → { "id": "GUID" }

PUT /inventory/performances/{performanceId}/capacity → resize capacityBody:

{ "newCapacity": 10 }

204 No Content

GET /inventory/performances/{id} → get snapshot200 OK → { "id": "...", "capacity": 10, "reserved": 3, "sold": 0 }

Sales

POST /sales/reservations → place reservationBody:

{ "performanceId": "GUID", "quantity": 3 }

201 Created → { "reservationId": "...", "performanceId": "...", "quantity": 3 }

POST /sales/reservations/{id}/confirm → confirm204 No Content

POST /sales/reservations/{id}/cancel → cancel204 No Content

Error mapping (middleware)

NotFoundException → 404

ConcurrencyException / invalid state → 409

DomainException (input/validation) → 422

otherwise → 500

Quality & conventions

References (onion):

Api → Application (+ Infrastructure for DI)

Application → Domain, Shared

Infrastructure → Application, Domain, Shared

Domain → (optionally) Shared only

No EF in API. Only IMediator calls.

Conventional Commits (suggested):

feat: initial Ticketing API with DDD layers (Api/Application/Domain/Infrastructure)

.gitignore / .dockerignore present

Swagger: Swashbuckle.AspNetCore + Microsoft.AspNetCore.OpenApi (v8.x for .NET 8)

MediatR license (v13+)

From MediatR v13, a license key is required. The Community license is free for eligible use (e.g., non‑production, education, small orgs), but you still need to register a key.

How we wire it (already in this repo):

Ticketing.Application.AddTicketingApplication(string? mediatrLicenseKey) accepts a key and sets cfg.LicenseKey.

Program.cs reads the key from configuration and passes it to the method.

Set the key locally (User Secrets – API project):

cd src/EventTickets.Api
 dotnet user-secrets init
 dotnet user-secrets set "MediatR:LicenseKey" "YOUR-KEY-HERE"

Set the key in production (environment variable):

# Linux/macOS
export MEDIATR__LICENSEKEY=YOUR-KEY-HERE
# Windows (PowerShell)
$env:MEDIATR__LICENSEKEY="YOUR-KEY-HERE"

We intentionally do not store the key in appsettings.json.

Troubleshooting

The specified transaction is not associated with the current connectionYou’re likely mixing DbContexts/connections. In this repo we use a single DbContext (Ticketing), so this should not occur.

NU1202 Microsoft.AspNetCore.OpenApi 9.x is not compatible with net8.0Install 8.x:dotnet add src/EventTickets.Api/EventTickets.Api.csproj package Microsoft.AspNetCore.OpenApi --version 8.0.x

Swagger doesn’t show operation metadataEnsure using Microsoft.AspNetCore.OpenApi; is present where you call WithOpenApi(...).

Git ignores not appliedUse:

git check-ignore -v some/path
git ls-files -i --exclude-standard

VS Code – REST Client (.http)

A requests.http file is provided at the root of the repository for testing the API with the VS Code REST Client extension (humao.rest-client). Install the extension, open requests.http, and click Send Request above each call.

Tip: set a fixed @performanceId in the file to avoid regenerating a new GUID across requests.

License

MIT

