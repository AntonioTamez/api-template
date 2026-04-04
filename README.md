# Company.Template API Starter

Opinionated .NET 8 template that combines Clean Architecture with hexagonal principles, Serilog-based observability, health checks, API rate limiting, EF Core + PostgreSQL persistence, Docker assets, and Dockerized k6 load testing.

## Highlights

- **Architecture**: Domain-driven core with `Result`/`Error` pattern, Application layer powered by MediatR + FluentValidation, Infrastructure adapters (logging, persistence, caching), and API endpoints/controller adapters.
- **Hexagonal**: Domain/Application expose ports (interfaces) while Infrastructure provides adapters (EF Core repositories, logging, etc.). Domain has no external references.
- **Production-ready middleware**: Health checks (`/health`, `/health/ready`), ProblemDetails, Serilog request logging, and configurable rate limiting via `appsettings` or environment variables.
- **Persistence**: EF Core 8 + Npgsql with dedicated `TemplateDbContext`, repository implementations, and migration scripts.
- **Docker-first**: Multi-stage Dockerfile, docker-compose stack (API + PostgreSQL + optional pgAdmin) plus observability services (Prometheus, Pushgateway, Grafana) and migration helpers.
- **Load testing**: `load-tests/k6` package ships with script, Dockerfile, docker-compose, `.env` template, and docs so any developer can run k6 locally—with optional Prometheus/Grafana dashboards.
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

4. Ensure PostgreSQL is running. The easiest option is to levantar el contenedor de docker-compose (solo el servicio de base de datos) antes de iniciar la API:

   ```bash
   docker compose up -d postgres
   ```

   Si prefieres usar otra instancia local, ajusta `Persistence:ConnectionString` en `appsettings.Development.json` para apuntar a ese host.

5. Apply database migrations (optional during first run):

   ```bash
   dotnet ef database update \
     --project Company.Template.Infrastructure.Persistence/Company.Template.Infrastructure.Persistence.csproj \
     --startup-project Company.Template.Api/Company.Template.Api.csproj
   ```

6. Launch the API (by default it listens on `http://localhost:5009` as defined in `Properties/launchSettings.json`):

   ```bash
   dotnet run --project Company.Template.Api/Company.Template.Api.csproj
   ```

   Si prefieres otro puerto, puedes ejecutar `ASPNETCORE_URLS=http://localhost:6000 dotnet run --project ...`.

7. Validate el servicio:
   - Health check: `curl http://localhost:5009/health/ready`
   - Sample endpoint (customer lookup): `curl http://localhost:5009/api/v1/customers/{customerId}`

The API exposes versioned routes like `POST /api/v1/customers`. Health endpoints live at `/health` (liveness) and `/health/ready` (readiness). Update las URLs si cambiaste el puerto (por ejemplo, al usar `ASPNETCORE_URLS`). On startup the `TemplateDbContextSeeder` runs automatically, applying migrations and inserting three demo customers (Ada Lovelace, Alan Turing, Grace Hopper) if the database is empty.

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

1. Copy the sample environment file: `cp .env.example .env` (compose will fail if `.env` is missing).
2. Adjust database/user/password/ports as needed.
3. Build and run the stack (API, Postgres, pgAdmin, Prometheus, Pushgateway, Grafana, postgres-exporter):

   ```bash
   docker compose up --build
   ```

The API will be reachable at `http://localhost:${API_HTTP_PORT:-8080}` and Postgres at `${DB_PORT:-5432}`. pgAdmin is available on `${PGADMIN_PORT:-8081}`. Prometheus lives at `http://localhost:${PROMETHEUS_PORT:-9090}`, Grafana at `http://localhost:${GRAFANA_PORT:-3000}` (default credentials `admin`/`admin`).

4. Validate the running containers:
    - `curl http://localhost:${API_HTTP_PORT:-8080}/health/ready`
    - `docker compose logs api -f`
    - Navigate to Grafana `/api-overview` or `/k6-overview` dashboards to observe metrics.

The same seeder runs inside the containerized API, so the demo customers appear the first time the stack boots with a clean database.

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

### Recommended (Prometheus-integrated)

1. Make sure the root `docker compose up` stack is running (Prometheus, Pushgateway, Grafana, API).
2. From the repository root execute:

   ```bash
   docker compose run --rm k6
   ```

   This uses the `k6` service defined in `docker-compose.yml`, streams metrics to the Pushgateway/Prometheus, and fills the Grafana “k6 Load Test” dashboard automatically.

3. Override defaults via environment variables (either edit `.env` or pass `K6_TARGET_URL=...` before the command).

### Standalone mode

If you only need console output, the legacy workflow is still available:

1. Navigate to `load-tests/k6` (este directorio contiene su propio `.env.example`).
2. Copy `.env.example` to `.env` and set `K6_TARGET_URL`, `K6_VUS`, `K6_DURATION`.
3. Run `docker compose up --build` inside that directory.
4. Stop with `Ctrl+C` and `docker compose down`.

Artifacts included:

- `scripts/smoke.js` – default scenario hitting the health endpoint.
- `Dockerfile` – wraps k6 in a portable container.
- `docker-compose.yml` – exposes the environment variables.
- `.env.example` – starting point for configuration.
- `README.md` – quick reference for developers.

## Observability (Prometheus + Loki + Grafana)

The main `docker-compose.yml` now launches Prometheus, Pushgateway, postgres-exporter, Alertmanager, Loki (logs), Promtail (log shipping), and Grafana alongside the API.

1. Start the stack: `docker compose up --build`.
2. Access Prometheus at `http://localhost:${PROMETHEUS_PORT:-9090}` to inspect targets and raw queries.
3. Access Loki at `http://localhost:${LOKI_PORT:-3100}` for raw log queries (or via Grafana's Explore).
4. Access Grafana at `http://localhost:${GRAFANA_PORT:-3000}` (credentials: `${GRAFANA_ADMIN_USER:-admin}` / `${GRAFANA_ADMIN_PASSWORD:-admin}`). Two dashboards are provisioned automatically:
   - **API Overview** – throughput, 5xx ratio, latency percentiles.
   - **k6 Load Test** – VUs, RPS, p50/p95 latency, check failures.
5. All health checks are also exported as Prometheus metrics thanks to `prometheus-net`.
6. Alert samples are defined in `prometheus/alerts.yml` (high API error rate, k6 p95 latency) and routed through Alertmanager (`http://localhost:${ALERTMANAGER_PORT:-9093}`). Customize `alertmanager/alertmanager.yml` with your email/webhook integrations.
7. Logs from all containers are automatically shipped to Loki via Promtail. In Grafana, switch the datasource to **Loki** in Explore and query using LogQL, e.g. `{job="api"} |= "error"` to find error messages.

When you run `docker compose run --rm k6`, Prometheus collects the k6 metrics via Pushgateway and Grafana updates automatically. You can add more panels/dashboards under `grafana/dashboards/` and they will be provisioned on container restart.

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
