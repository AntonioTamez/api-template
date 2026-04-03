# Company.Template API Starter

Opinionated .NET 8 template that combines Clean Architecture with hexagonal principles, Serilog-based observability, health checks, API rate limiting, EF Core + PostgreSQL persistence, Docker assets, and Dockerized k6 load testing.

## Highlights

- **Architecture**: Domain-driven core with `Result`/`Error` pattern, Application layer powered by MediatR + FluentValidation, Infrastructure adapters (logging, persistence, caching), and API endpoints/controller adapters.
- **Hexagonal**: Domain/Application expose ports (interfaces) while Infrastructure provides adapters (EF Core repositories, logging, etc.). Domain has no external references.
- **Production-ready middleware**: Health checks (`/health`, `/health/ready`), ProblemDetails, Serilog request logging, and configurable rate limiting via `appsettings` or environment variables.
- **Persistence**: EF Core 8 + Npgsql with dedicated `TemplateDbContext`, repository implementations, and migration scripts.
- **Docker-first**: Multi-stage Dockerfile, docker-compose stack (API + PostgreSQL + optional pgAdmin), and migration helpers.
- **Load testing**: `load-tests/k6` package ships with script, Dockerfile, docker-compose, `.env` template, and docs so any developer can run k6 locally with one command.
- **Easy renaming**: PowerShell and Bash scripts update file contents, directories, and project names from `Company.Template` to any other solution prefix.

## Project structure

```
Company.Template.Api/                 # ASP.NET Core API (controllers, DI setup, middleware)
Company.Template.Application/         # Application layer (MediatR, commands/queries, validators)
Company.Template.Domain/              # Entities, value objects, Result/Error pattern, repository contracts
Company.Template.Infrastructure/      # Cross-cutting infrastructure + registration glue
Company.Template.Infrastructure.Persistence/  # EF Core DbContext, configurations, repositories
Company.Template.*.UnitTests/         # Sample unit tests per layer
load-tests/k6/                        # Dockerized k6 load-testing bundle
scripts/                              # Renaming + migration scripts
```

## Prerequisites

- .NET SDK `8.0.419` (enforced via `global.json`).
- Docker (latest stable) for container workflows.
- Node dependencies are **not** required.

## Local development (step by step)

1. Restore dependencies:

   ```bash
   dotnet restore
   ```

2. Build the solution:

   ```bash
   dotnet build
   ```

3. Run the full test suite:

   ```bash
   dotnet test
   ```

4. Apply database migrations (optional during first run):

   ```bash
   dotnet ef database update \
     --project Company.Template.Infrastructure.Persistence/Company.Template.Infrastructure.Persistence.csproj \
     --startup-project Company.Template.Api/Company.Template.Api.csproj
   ```

5. Launch the API:

   ```bash
   dotnet run --project Company.Template.Api/Company.Template.Api.csproj
   ```

6. Validate the service:
   - Health check: `curl http://localhost:5187/health/ready`
   - Sample endpoint (customer lookup): `curl http://localhost:5187/api/v1/customers/{customerId}`

The API exposes versioned routes like `POST /api/v1/customers`. Health endpoints live at `/health` (liveness) and `/health/ready` (readiness). Update the port (`5187`) if you configured a different one.

### Database migrations

Use the helper scripts (optional arguments let you override project paths or environment):

```powershell
pwsh ./scripts/run-migrations.ps1
```

```bash
./scripts/run-migrations.sh
```

Both scripts execute `dotnet ef database update` using `Company.Template.Api` as the startup project.

### Docker workflow

1. Copy the sample environment file: `cp .env.example .env`.
2. Adjust database/user/password/ports as needed.
3. Build and run the stack:

   ```bash
   docker compose up --build
   ```

The API will be reachable at `http://localhost:${API_HTTP_PORT:-8080}` and Postgres at `${DB_PORT:-5432}`. pgAdmin is available on `${PGADMIN_PORT:-8081}`.

4. Validate the running containers:
   - `curl http://localhost:${API_HTTP_PORT:-8080}/health/ready`
   - `docker compose logs api -f`

### Renaming the template

Run one of the provided scripts from the repository root:

```powershell
pwsh ./scripts/rename-solution.ps1 Contoso.Service
```

```bash
./scripts/rename-solution.sh Contoso.Service
```

Both scripts replace `Company.Template` across text files and rename folders/projects to match the new solution name.

## Load testing with Dockerized k6

1. Navigate to `load-tests/k6`.
2. Copy `.env.example` to `.env` and set:
   - `K6_TARGET_URL` – API endpoint to hit (e.g., `http://host.docker.internal:8080/health/ready`).
   - `K6_VUS` – concurrent users.
   - `K6_DURATION` – duration such as `30s`, `5m`.
3. Launch the test with `docker compose up --build`.
4. Watch the console output for the final summary (k6 prints metrics like `http_req_duration`, `vus`, `checks`).
5. Stop the test with `Ctrl+C` and tear down containers using `docker compose down`.

Artifacts included:

- `scripts/smoke.js` – default scenario hitting the health endpoint.
- `Dockerfile` – wraps k6 in a portable container.
- `docker-compose.yml` – exposes the environment variables.
- `.env.example` – starting point for configuration.
- `README.md` – quick reference for developers.

## Rate limiting & configuration

`appsettings.json` defines the default rate limiter (fixed window). Override via environment variables, e.g.:

```bash
export RateLimiting__PermitLimit=200
export RateLimiting__WindowInSeconds=30
```

Persistence settings live under `Persistence:ConnectionString` and can be overridden with `Persistence__ConnectionString` when running in Docker or against other database engines.

## Next steps

- Add more aggregates by following the Domain → Application → Infrastructure flow demonstrated with `Customer`.
- Introduce additional ports/adapters (messaging, caching) inside Infrastructure as needed.
- Extend integration tests under `Company.Template.Infrastructure.IntegrationTests` to cover persistence scenarios.
- Update CI (e.g., GitHub Actions) to run `dotnet format`, `dotnet test`, Docker build, and optional k6 smoke tests.
