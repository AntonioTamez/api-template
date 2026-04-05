# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Company.Template API** is a production-ready .NET 8 API template built with Clean Architecture + Hexagonal (Ports & Adapters) principles. The project is designed to be renamed via scripts and extended following established patterns.

## Commands

### Build & Run
```bash
dotnet restore
dotnet build
dotnet run --project src/Api/Company.Template.Api.csproj
```

### Tests
```bash
dotnet test                                                    # All tests
dotnet test tests/Domain.UnitTests/                           # Domain unit tests only
dotnet test tests/Application.UnitTests/                      # Application unit tests only
dotnet test tests/Infrastructure.IntegrationTests/            # Integration tests (requires DB)
dotnet test --filter "FullyQualifiedName~ClassName.MethodName"  # Single test
```

### Database Migrations
```bash
# Via scripts
./scripts/run-migrations.sh         # Bash
pwsh ./scripts/run-migrations.ps1   # PowerShell

# Or directly
dotnet ef database update --project src/Infrastructure.Persistence --startup-project src/Api
```

### Docker
```bash
docker compose up --build           # Full stack (API, DB, monitoring)
docker compose up -d postgres       # Database only
docker compose run --rm k6          # Run k6 load tests
```

### Rename Template
```bash
./scripts/rename-solution.sh <NewCompany> <NewProject>
pwsh ./scripts/rename-solution.ps1 <NewCompany> <NewProject>
```

## Architecture

### Layer Structure

```
src/
├── Domain/                    # Entities, Value Objects, Domain Events, Result pattern
├── Application/               # MediatR Commands/Queries, FluentValidation, Use cases
├── Infrastructure.Persistence/ # EF Core, Repositories, Migrations, Seeder
├── Infrastructure/            # Cross-cutting infrastructure wiring
└── Api/                       # ASP.NET Core controllers, middleware, DI setup

tests/
├── Domain.UnitTests/
├── Application.UnitTests/
└── Infrastructure.IntegrationTests/
```

### Key Patterns

**Result Pattern** — All operations return `Result` or `Result<T>`. Never throw exceptions for business failures; define errors in `Domain/Errors/`.

**Ports & Adapters** — Interfaces (ports) live in `Domain` or `Application`; implementations (adapters) live in `Infrastructure.Persistence`. Never reference infrastructure from domain/application.

**MediatR Pipeline** — Commands/Queries flow through `ValidationBehavior` before handlers. Add new behaviors in `Application/Behaviors/`.

**Messaging marker interfaces** — All commands implement `ICommand` or `ICommand<TResponse>` and all queries implement `IQuery<TResponse>` (in `Application/Abstractions/Messaging/`). Handlers use `ICommandHandler<TCommand>` / `IQueryHandler<TQuery, TResponse>`.

**IUnitOfWork** — `TemplateDbContext` implements `IUnitOfWork` and is registered under that interface in DI. Handlers inject `IUnitOfWork` to call `SaveChangesAsync`.

**Error → HTTP status mapping** — Controllers map `Result.Error` to HTTP status using a convention: if `error.Code` contains `"NotFound"` → 404, otherwise → 400. Define errors in `Domain/Errors/`.

**`Infrastructure` vs `Infrastructure.Persistence`** — `Infrastructure` is a thin DI aggregation layer (`AddInfrastructure` delegates to `AddPersistence`). All actual EF Core, repositories, and migrations live in `Infrastructure.Persistence`.

**Adding a new aggregate** — Follow the `Customer` example:
1. `Domain/` — Entity + Id ValueObject + domain events + `IRepository` interface + error definitions in `Domain/Errors/`
2. `Application/` — Commands + Queries + Handlers + Validators + Response DTOs
3. `Infrastructure.Persistence/` — EF configuration + Repository implementation
4. `Api/Controllers/` — Controller calling MediatR; map errors via `ToActionResult(error)`

### Observability Stack (Docker only)

```
API Logs   → Serilog → Promtail → Loki → Grafana
API Metrics → /metrics → Prometheus → Grafana
k6 Metrics → Pushgateway → Prometheus → Grafana
```

Services: Prometheus (9090), Grafana (3000), Loki (3100), PgAdmin (8081), Alertmanager (9093).

### API Endpoints

| Method | Path | Description |
|--------|------|-------------|
| POST | `/api/v1/customers` | Register customer |
| GET | `/api/v1/customers/{id}` | Get customer by ID |
| GET | `/health` | Liveness probe |
| GET | `/health/ready` | Readiness probe (includes DB) |
| GET | `/metrics` | Prometheus metrics |
| GET | `/swagger` | Swagger UI |

## Configuration

- Copy `.env.example` to `.env` before running Docker
- `appsettings.json` — Serilog, Persistence connection string, RateLimiting
- Rate limiting is fixed-window per IP; override via env: `RateLimiting__PermitLimit=200`
- API versioning is **header-based**: send `X-Api-Version: 1.0` (defaults to 1.0 when omitted)
- Database seeder runs on startup and inserts 3 demo customers if DB is empty
- `global.json` enforces .NET SDK 8.0.419
- `Directory.Build.props` enforces nullable reference types, `TreatWarningsAsErrors=true`
