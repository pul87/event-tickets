# EventTickets

A modular monolith implemented with **DDD** (Domain/Application/Infrastructure), **CQRS with MediatR**, and **EF Core** on PostgreSQL.\
Two Bounded Contexts: **Ticketing** and **Payments** with separate DB schemas: `ticketing` and `payments`.

## Table of contents

- [Architecture](#architecture)
- [Project structure](#project-structure)
- [Getting started](#getting-started)
- [Run](#run)
- [Database & migrations](#database--migrations)
- [API](#api)
- [Quality & conventions](#quality--conventions)
- [Troubleshooting](#troubleshooting)
- [License](#license)

---

## Architecture

- **Modular Monolith** with two BCs: `Ticketing` and `Payments`
- **DDD layers**
  - **Domain**: aggregates, invariants, value objects, domain exceptions
  - **Application**: commands/queries + handlers (`MediatR`), ports (repos/UoW)
  - **Infrastructure**: EF Core DbContexts, repositories, UoW, DI extensions
  - **API**: minimal APIs only call `IMediator`; no EF usage here
- **Persistence**: PostgreSQL with separate schemas (`ticketing`, `payments`), optimistic concurrency via Postgres `xmin` (`IsRowVersion()`)
- **Integration**: Cross-BC communication via integration events and outbox pattern

## Project structure

```
src/
  EventTickets.Api/                     # Web API (composition root)
    Endpoints/
      InventoryEndpoints.cs
      SalesEndpoints.cs
      PaymentsEnpoints.cs
    ExceptionMappingMiddleware.cs
  EventTickets.Ticketing.Application/   # Ticketing use cases (MediatR), ports
    Abstractions/
    Inventory/ (Create/Resize/Get)
    Reservations/ (Place/Confirm/Cancel)
    IntegrationHandlers/
    DependencyInjection.cs              # AddTicketingApplication()
  EventTickets.Ticketing.Domain/        # Ticketing aggregates + rules
    PerformanceInventory.cs
    Reservation.cs
  EventTickets.Ticketing.Infrastructure/# Ticketing EF Core & DI
    TicketingDbContext.cs
    Persistence/
    Outbox/
    ServiceCollectionExtensions.cs      # AddTicketingModule()
  EventTickets.Payments.Application/    # Payments use cases (MediatR), ports
    Abstractions/
    PaymentIntents/ (Get/ProcessWebhook)
    IntegrationHandlers/
    DependencyInjection.cs              # AddPaymentsApplication()
  EventTickets.Payments.Domain/         # Payments aggregates + rules
    PaymentIntent.cs
  EventTickets.Payments.Infrastructure/# Payments EF Core & DI
    PaymentsDbContext.cs
    Persistence/
    Outbox/
    ServiceCollectionExtensions.cs      # AddPaymentsModule()
  EventTickets.Shared/                  # Cross-cutting concerns
    AggregateRoot.cs, DomainException.cs
    IntegrationEvents/
    Outbox/
```

---

## Getting started

### Prerequisites

- .NET **8.0** SDK
- Docker (for PostgreSQL)
- PowerShell or Bash

### Configure database

Set the connection string in `src/EventTickets.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=eventtickets;Username=et;Password=etpwd"
  }
}
```

Bring up Postgres with Adminer (example `docker-compose.yml`):

```yaml
services:
  postgres:
    image: postgres:16-alpine
    container_name: eventtickets-postgres
    environment:
      POSTGRES_USER: et
      POSTGRES_PASSWORD: etpwd
      POSTGRES_DB: eventtickets
    ports:
      - "5432:5432"
    volumes:
      - pgdata:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U et -d eventtickets"]
      interval: 5s
      timeout: 5s
      retries: 5
  adminer:
    image: adminer
    container_name: eventtickets-adminer
    ports:
      - "8080:8080"
    depends_on:
      postgres:
        condition: service_healthy
volumes:
  pgdata:
```

```bash
docker compose up -d
```

---

## Run

```bash
dotnet build
dotnet run --project src/EventTickets.Api
# Swagger UI → http://localhost:5000/swagger
# Health     → http://localhost:5000/health
# Adminer    → http://localhost:8080 (database admin)
```

> If your ASP.NET profile binds to a different port, adjust Postman/Swagger accordingly.

---

## Database & migrations

Migrations live in **Infrastructure** projects.

### Ticketing migrations

Create a migration:

```bash
dotnet ef migrations add InitialTicketing \
  -p src/EventTickets.Ticketing.Infrastructure \
  -s src/EventTickets.Api \
  -o Migrations
```

Apply:

```bash
dotnet ef database update \
  -p src/EventTickets.Ticketing.Infrastructure \
  -s src/EventTickets.Api
```

### Payments migrations

Create a migration:

```bash
dotnet ef migrations add InitialPayments \
  -p src/EventTickets.Payments.Infrastructure \
  -s src/EventTickets.Api \
  -o Migrations
```

Apply:

```bash
dotnet ef database update \
  -p src/EventTickets.Payments.Infrastructure \
  -s src/EventTickets.Api
```

> Tip: If you prefer design-time independence, add an `IDesignTimeDbContextFactory` in each Infrastructure project.

---

## API

### Inventory

- **POST** `/inventory/performances` → create capacity\
  Body:

  ```json
  { "performanceId": "GUID", "capacity": 5 }
  ```

  `201 Created` → `{ "id": "GUID" }`

- **PUT** `/inventory/performances/{performanceId}/capacity` → resize capacity\
  Body:

  ```json
  { "newCapacity": 10 }
  ```

  `204 No Content`

- **GET** `/inventory/performances/{id}` → get snapshot\
  `200 OK` → `{ "id": "...", "capacity": 10, "reserved": 3, "sold": 0 }`

### Sales

- **POST** `/sales/reservations` → place reservation\
  Body:

  ```json
  { "performanceId": "GUID", "quantity": 3 }
  ```

  `201 Created` → `{ "reservationId": "...", "performanceId": "...", "quantity": 3 }`

- **POST** `/sales/reservations/{id}/confirm` → confirm\
  `204 No Content`

- **POST** `/sales/reservations/{id}/cancel` → cancel\
  `204 No Content`

### Payments

- **GET** `/payments/intents/{reservationId}` → get payment intent\
  `200 OK` → `{ "id": "...", "reservationId": "...", "amount": 150.00, "payUrl": "...", "status": "Requested" }`

- **POST** `/payments/webhooks` → process payment webhook\
  Body:

  ```json
  {
    "eventType": "PaymentSucceeded",
    "paymentIntentId": "GUID",
    "providerTransactionId": "txn_123",
    "amount": 150.00,
    "timestamp": "2024-01-01T00:00:00Z"
  }
  ```

  `200 OK` → `{ "success": true, "newStatus": "Captured" }`

### Error mapping (middleware)

- `NotFoundException` → **404**
- `ConcurrencyException` / invalid state → **409**
- `DomainException` (input/validation) → **422**
- otherwise → **500**

---

## Quality & conventions

- **References (onion):**
  - Api → Application (+ Infrastructure for DI)
  - Application → Domain, Shared
  - Infrastructure → Application, Domain, Shared
  - Domain → (optionally) Shared only
- **No EF in API.** Only `IMediator` calls.
- **Cross-BC Communication**: Integration events via outbox pattern
- **Conventional Commits** (suggested):
  - `feat: add Payments BC with webhook processing`
- **.gitignore / .dockerignore** present
- **Swagger**: `Swashbuckle.AspNetCore` + `Microsoft.AspNetCore.OpenApi` (v8.x for .NET 8)

---

## MediatR license (v13+)

From MediatR **v13**, a license key is required. The **Community** license is free for eligible use (e.g., non‑production, education, small orgs), but you still need to register a key.

**How we wire it (already in this repo):**

- `Ticketing.Application.AddTicketingApplication(string? mediatrLicenseKey)` and `Payments.Application.AddPaymentsApplication(string? mediatrLicenseKey)` accept a key and set `cfg.LicenseKey`.
- `Program.cs` reads the key from configuration and passes it to both methods.

**Set the key locally (User Secrets – API project):**

```bash
cd src/EventTickets.Api
 dotnet user-secrets init
 dotnet user-secrets set "MediatR:LicenseKey" "YOUR-KEY-HERE"
```

**Set the key in production (environment variable):**

```bash
# Linux/macOS
export MEDIATR__LICENSEKEY=YOUR-KEY-HERE
# Windows (PowerShell)
$env:MEDIATR__LICENSEKEY="YOUR-KEY-HERE"
```

> We intentionally **do not** store the key in `appsettings.json`.

---

## Troubleshooting

- **DbContext conflicts**\
  You're likely mixing DbContexts/connections. In this repo we use **two DbContexts** (Ticketing, Payments) with separate schemas, so ensure proper connection string configuration.

- ``\
  Install **8.x**:\
  `dotnet add src/EventTickets.Api/EventTickets.Api.csproj package Microsoft.AspNetCore.OpenApi --version 8.0.x`

- **Swagger doesn’t show operation metadata**\
  Ensure `using Microsoft.AspNetCore.OpenApi;` is present where you call `WithOpenApi(...)`.

- **Git ignores not applied**\
  Use:

  ```bash
  git check-ignore -v some/path
  git ls-files -i --exclude-standard
  ```

---

## VS Code – REST Client (.http)

A `` file is provided at the **root of the repository** for testing the API with the VS Code **REST Client** extension (humao.rest-client). Install the extension, open `requests.http`, and click **Send Request** above each call.

> Tip: set a fixed `@performanceId` in the file to avoid regenerating a new GUID across requests.

## License

MIT

