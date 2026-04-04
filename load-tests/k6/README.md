# k6 Load Tests

This package lets you run k6 load tests inside Docker without installing k6 locally.

## Structure

- `scripts/smoke.js` – example scenario that hits the API health endpoint.
- `Dockerfile` – packages the scripts into a runnable container.
- `docker-compose.yml` – bootstraps the k6 container with configuration from environment variables.
- `.env.example` – sample configuration for the compose stack.

## Configuration

Copy the example file and adjust the values as needed:

```bash
cp .env.example .env
```

Available variables:

| Name | Description | Default |
| --- | --- | --- |
| `K6_TARGET_URL` | API endpoint under test | `http://host.docker.internal:8080/health/ready` |
| `K6_VUS` | Concurrent virtual users | `10` |
| `K6_DURATION` | Duration (e.g. `30s`, `5m`) | `30s` |

## Run the test (standalone)

From `load-tests/k6` run:

```bash
docker compose up --build
```

To stop the test, press `Ctrl+C` and remove the container with `docker compose down`.

## Run the test against the observability stack

If you already have the root `docker compose up` stack running (which includes Prometheus, Pushgateway, and Grafana), you can trigger the same scenario while streaming metrics directly into Prometheus with:

```bash
docker compose run --rm k6
```

This command must be executed from the repository root. It uses the `k6` service defined in the main `docker-compose.yml`, forwards metrics to the Pushgateway, and makes them visible in Grafana's k6 dashboard automatically.
