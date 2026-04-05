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
src/
├── Api/                        # ASP.NET Core API (controllers, DI setup, middleware)
├── Application/                # Application layer (MediatR, commands/queries, validators)
├── Domain/                     # Entities, value objects, Result/Error pattern, repository contracts
├── Infrastructure/             # Cross-cutting infrastructure + registration glue
└── Infrastructure.Persistence/ # EF Core DbContext, configurations, repositories
tests/
├── Domain.UnitTests/
├── Application.UnitTests/
└── Infrastructure.IntegrationTests/
load-tests/k6/                  # Dockerized k6 load-testing bundle
scripts/                        # Renaming + migration scripts
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

4. Ensure PostgreSQL is running. The easiest option is to start only the database container before launching the API:

   ```bash
   docker compose up -d postgres
   ```

   If you prefer a local instance, update `Persistence:ConnectionString` in `appsettings.Development.json` to point to that host.

5. Apply database migrations (optional during first run):

   ```bash
   dotnet ef database update --project src/Infrastructure.Persistence/Company.Template.Infrastructure.Persistence.csproj --startup-project src/Api/Company.Template.Api.csproj
   ```

6. Launch the API (by default it listens on `http://localhost:5009` as defined in `Properties/launchSettings.json`):

   ```bash
   dotnet run --project src/Api/Company.Template.Api.csproj
   ```

   To use a different port: `ASPNETCORE_URLS=http://localhost:6000 dotnet run --project src/Api/Company.Template.Api.csproj`.

7. Validate the service:
   - Health check: `curl http://localhost:5009/health/ready`
   - Sample endpoint (customer lookup): `curl -H "X-Api-Version: 1.0" http://localhost:5009/api/customers/{customerId}`

The API uses header-based versioning (`X-Api-Version: 1.0`). Routes follow the pattern `POST /api/customers`. Health endpoints live at `/health` (liveness) and `/health/ready` (readiness). Update the URLs if you changed the port (e.g. via `ASPNETCORE_URLS`). On startup the `TemplateDbContextSeeder` runs automatically, applying migrations and inserting three demo customers (Ada Lovelace, Alan Turing, Grace Hopper) if the database is empty.

### Database migrations

Use the helper scripts (optional arguments let you override project paths or environment):

```powershell
pwsh ./scripts/run-migrations.ps1
```

```bash
./scripts/run-migrations.sh
```

Both scripts execute `dotnet ef database update` using `src/Api` as the startup project.

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

1. Navigate to `load-tests/k6` (this directory contains its own `.env.example`).
2. Copy `.env.example` to `.env` and set `K6_TARGET_URL`, `K6_VUS`, `K6_DURATION`.
3. Run `docker compose up --build` inside that directory.
4. Stop with `Ctrl+C` and `docker compose down`.

Artifacts included:

- `scripts/smoke.js` – default scenario hitting the health endpoint.
- `Dockerfile` – wraps k6 in a portable container.
- `docker-compose.yml` – exposes the environment variables.
- `.env.example` – starting point for configuration.
- `README.md` – quick reference for developers.

## Observability (Logging, Metrics, Traces & Dashboards)

The template includes a complete observability stack: **Serilog** for structured logging, **Prometheus** for metrics, **Loki** for log centralization, and **Grafana** for visualization with pre-configured dashboards.

### Structured Logging with Serilog

The API uses **Serilog** as its structured logging library, configured in `Program.cs`:

```csharp
builder.Host.UseSerilog((context, loggerConfiguration) => loggerConfiguration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());
```

**Features:**
- Configuration read from `appsettings.json` (`Serilog` section).
- Automatic enrichment with HTTP context (request ID, method, path, status code, duration).
- Console sink enabled by default.
- Additional sinks (File, Seq, etc.) can be added in `appsettings` or via code.

**Override via environment:**
```bash
export Serilog__WriteTo__0__Args__outputTemplate="{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
```

### Metrics with Prometheus

The API exposes Prometheus-format metrics via `prometheus-net`:

- Metrics endpoint: `GET /metrics` (auto-mapped by `app.MapMetrics()`).
- Automatic HTTP metrics: requests, durations, response codes.
- Health checks as metrics: Postgres and other component status.

**Prometheus integration (docker-compose):**
- Prometheus scrapes `/metrics` from the `api` container.
- Pushgateway receives k6 metrics during load tests.

**Available metrics:**
- `http_requests_total` – request counter by method, endpoint, status.
- `http_request_duration_seconds` – latency histogram.
- `aspnetcore_health_checks_*` – health check status.

### Log Centralization with Loki

All containers ship logs to **Loki** via **Promtail**:

1. **Promtail** listens to Docker container logs at `/var/lib/docker/containers`.
2. **Loki** stores and indexes logs.
3. **Grafana** queries Loki via configured datasource.

**Example LogQL queries in Grafana:**
- All API logs: `{job="api"}`
- Errors only: `{job="api"} |= "error"`
- By level: `{job="api"} | json | level="error"`

### Grafana Dashboards

Two dashboards auto-provisioned:

| Dashboard | Content |
|-----------|--------|
| **API Overview** | Throughput (req/s), 5xx error ratio, latency (p50, p95, p99), memory usage, DB connections |
| **k6 Load Test** | Active Virtual Users, requests/s, latency (p50, p95), failed checks rate, HTTP errors |

**Access:** `http://localhost:${GRAFANA_PORT:-3000}` (admin/admin)

### Alerts and Alertmanager

Alerts configured in `monitoring/prometheus/alerts.yml`:

- `HighApiErrorRate`: >5% 5xx responses in 5 minutes.
- `HighK6Latency`: p95 latency >1s.
- `ApiDown`: API unavailable.

**Notification configuration:** Edit `monitoring/alertmanager/alertmanager.yml` with email, webhook, Slack, etc.

### Complete Observability Pipeline

```
┌─────────┐    ┌─────────────┐    ┌──────────┐    ┌─────────┐
│   API   │───▶│  Serilog    │───▶│ Promtail │───▶│  Loki   │
│ (logs)  │    │  (Console)  │    │          │    │         │
└─────────┘    └─────────────┘    └──────────┘    └─────────┘
       │
       │ /metrics
       ▼
┌─────────────┐    ┌─────────────┐    ┌─────────┐
│ Prometheus  │◀───│  Pushgateway │◀───│   k6    │
│  (scrapes) │    │  (batch)    │    │ (tests) │
└─────────────┘    └─────────────┘    └─────────┘
       │
       ▼
┌─────────────────────────────────────────────────┐
│                Grafana                          │
│  - Dashboards (API, k6)                        │
│  - Explore (Loki logs)                        │
│  - Alerts (Alertmanager)                      │
└─────────────────────────────────────────────────┘
```

### Custom Configuration

**Adding extra Serilog sinks (e.g., File, Seq):**

In `appsettings.json`:
```json
"Serilog": {
  "WriteTo": [
    { "Name": "Console" },
    { "Name": "File", "Args": { "path": "logs/api-.log" } }
  ]
}
```

**Enabling OpenTelemetry (traces):**
```bash
export OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317
```
Add package `OpenTelemetry.Exporter.OpenTelemetryProtocol` and configure in `Program.cs`.

### Observability Validation

```bash
# View raw metrics
curl http://localhost:8080/metrics

# View health checks
curl http://localhost:8080/health/ready

# Query logs in Loki (via Grafana Explore)
{job="api"} |= "error"

# View targets in Prometheus
http://localhost:9090/targets
```

## Rate limiting & configuration

`appsettings.json` defines the default rate limiter (fixed window). Override via environment variables, e.g.:

```bash
export RateLimiting__PermitLimit=200
export RateLimiting__WindowInSeconds=30
```

Persistence settings live under `Persistence:ConnectionString` and can be overridden with `Persistence__ConnectionString` when running in Docker or against other database engines.

## CI/CD

### Workflows

| Workflow | Trigger | Jobs |
|----------|---------|------|
| **CI** (`.github/workflows/ci.yml`) | Push / PR to `main` | Build → Unit tests → Integration tests (Postgres service) → Docker build |
| **Docker Publish** (`.github/workflows/docker-publish.yml`) | Push tag `v*.*.*` | Tests → Build & push image to `ghcr.io` |

### Docker image

Images are published to GitHub Container Registry under `ghcr.io/<owner>/<repo>` and tagged automatically from the git tag:

| Git tag | Docker tags |
|---------|-------------|
| `v1.2.3` | `1.2.3`, `1.2`, `1`, `sha-<short>` |

### Releasing a new version

```bash
git tag v1.0.0
git push origin v1.0.0
```

The `docker-publish` workflow picks up the tag, runs the unit tests, and pushes the image.

### Required secrets

The workflow uses `GITHUB_TOKEN` (automatically provided by GitHub Actions) to push to GHCR — no extra secrets needed.

## Next steps

- Add more aggregates by following the Domain → Application → Infrastructure flow demonstrated with `Customer`.
- Introduce additional ports/adapters (messaging, caching) inside Infrastructure as needed.
- Extend integration tests under `Company.Template.Infrastructure.IntegrationTests` to cover persistence scenarios.
- Update CI (e.g., GitHub Actions) to run `dotnet format`, `dotnet test`, Docker build, and optional k6 smoke tests.
