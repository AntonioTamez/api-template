## Objetivo

Crear un template de API con .NET 8 que aplique Clean Architecture y arquitectura hexagonal, con dominio independiente, dockerizado junto a Postgres pero fácilmente intercambiable por otro motor, incluyendo health checks, rate limiting, patrón Result y un paquete de pruebas de carga k6 ejecutable vía contenedores.

## Estructura de la solución

- `Company.Template.sln`
- `Directory.Build.props` / `Directory.Build.targets` con metadatos comunes (nullable, LangVersion, root namespace, versión) para que renombrar la solución sea sencillo.
- Proyectos principales:
  - `Company.Template.Api`
  - `Company.Template.Application`
  - `Company.Template.Domain`
  - `Company.Template.Infrastructure`
  - `Company.Template.Infrastructure.Persistence`
- Proyectos de pruebas:
  - `Company.Template.Domain.UnitTests`
  - `Company.Template.Application.UnitTests`
  - `Company.Template.Infrastructure.IntegrationTests`

## Arquitectura (Clean + Hexagonal)

- **Dominio**: entidades, agregados, value objects, eventos, interfaces de repositorio, errores y `Result`. Sin dependencias externas.
- **Aplicación**: casos de uso (commands/queries) con MediatR, validación (FluentValidation), servicios de dominio, puertos (interfaces) para adaptadores salientes.
- **Infraestructura**: adaptadores concretos (EF Core, mensajería, cache, logging). Configuración agrupada en módulos de DI (`AddInfrastructure`, `AddPersistence`).
- **API**: controlador o minimal API como adaptador entrante. Expone DTOs, configura middlewares (rate limiting, ProblemDetails, health checks), versiona endpoints (`/api/v1`).
- **Hexagonal**: puertos definidos en Domain/Application; adaptadores entrantes (API, workers) y salientes (repositorios, proveedores externos) viven en Infrastructure.

## Patrón Result

- Implementar `Result` y `Result<T>` en `Domain.Shared` con estados `IsSuccess/IsFailure`, colección de errores (`Error` con `Code` y `Message`).
- Métodos helper (`Success`, `Failure`, `Map`, `Bind`).
- Usar en handlers de Application para evitar excepciones controladas y estandarizar respuestas.

## Health Checks y Rate Limiting

- Health checks (`AspNetCore.Diagnostics.HealthChecks`) para self, Postgres y dependencias externas, expuestos en `/health` (liveness) y `/health/ready` (readiness).
- Rate limiting con middleware nativo .NET 8 (`AddRateLimiter`) usando política configurable via `appsettings` (token bucket o fixed window). Permitir override por endpoint y configuración mediante `Options`.

## Persistencia y Base de Datos

- `Infrastructure.Persistence` contiene `TemplateDbContext`, migraciones, `IUnitOfWork` y repositorios EF Core implementando puertos del dominio.
- Connection strings y proveedor definidos en `appsettings` y `.env`. Diseñar `IDatabaseProvider`/`PersistenceOptions` para facilitar cambio a SQL Server, MySQL, etc.
- Scripts para ejecutar migraciones dentro y fuera de Docker.

## Dockerización

- **API**: Dockerfile multi-stage (`dotnet restore`, `build`, `publish`, runtime `aspnet`).
- **docker-compose.yml** (raíz): servicios `api`, `postgres`, `pgadmin` (opcional). Variables en `.env` (`DB_HOST`, `DB_PORT`, `DB_USER`, `DB_PASSWORD`, `DB_NAME`).
- Health checks configurados para que Docker Compose espere a Postgres antes de iniciar la API.
- Documentar `docker compose up --build` y cómo cambiar de motor.

## Observabilidad y Configuración

- Logging estructurado (Serilog) con sinks configurables.
- OpenTelemetry opcional (traces/metrics) con exporters configurables.
- Configuración vía `Options` + validación (`ValidateOnStart`).
- Middleware de excepciones que transforma fallos a `ProblemDetails` compatibles con `Result`.

## Automatización

- Scripts `dotnet format`, `dotnet test`, `dotnet ef migrations add`.
- GitHub Actions: pipeline con restore/build/test/lint y job opcional para ejecutar contenedor k6 contra entorno configurable.
- Scripts de renombrado (`scripts/rename-solution.ps1` y `.sh`): solicitan nuevo nombre y reemplazan `Company.Template` en `.sln`, `.csproj`, namespaces, Dockerfiles, compose, README, archivos k6.

## Paquete de pruebas de carga con k6

```
load-tests/
  k6/
    scripts/
      smoke.js
    Dockerfile
    docker-compose.yml
    .env.example
    README.md
```

- **Variables**: `K6_TARGET_URL`, `K6_VUS`, `K6_DURATION` (exportadas vía `.env` o línea de comandos).
- **Script (`smoke.js`)**: usa `__ENV` para leer variables, configura `options` y realiza peticiones GET (u otras) verificando respuestas con `check`.
- **Dockerfile**: basado en `grafana/k6`, copia scripts y define `ENTRYPOINT` (`k6 run /tests/scripts/smoke.js`).
- **docker-compose**: construye imagen local, monta `.env`, permite ejecutar `docker compose up --build` sin instalar k6.
- **README**: explica configuración, variables, comando de ejecución y cómo apuntar al API (ej. `http://host.docker.internal:5000/api/v1/health`).

## Documentación

- README raíz: resumen de arquitectura, pasos para levantar el proyecto (local y Docker), renombrado, health checks, rate limiting, migraciones, pruebas k6.
- README en `load-tests/k6` con instrucciones detalladas.
- Explicar cómo agregar nuevos módulos (puertos/adaptadores) y cómo cambiar proveedor de base de datos.

## Pasos sugeridos para el desarrollo

1. Inicializar repo Git y crear solución/proyectos.
2. Configurar `Directory.Build.props/targets`, `global.json` (SDK 8), packages comunes.
3. Implementar dominio base (Result, Error, entidades ejemplo) y pruebas unitarias.
4. Añadir Application (MediatR, comportamientos, validaciones) + pruebas.
5. Configurar Infrastructure + Persistence, EF Core y Postgres.
6. Crear API con endpoints de ejemplo, rate limiting, health checks, ProblemDetails.
7. Crear Dockerfile + docker-compose + scripts de migraciones.
8. Añadir paquete k6 (estructura, Dockerfile, compose, README).
9. Escribir documentación final (README, guías de renombrado, instrucciones de uso).
10. Configurar CI (GitHub Actions) y scripts de automatización.

Este plan sirve como guía para que cualquier desarrollador arranque la implementación manteniendo las buenas prácticas acordadas.
