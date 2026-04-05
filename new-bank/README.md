# Internet Bank – Case Study

A full-stack containerised application demonstrating real-world Docker and Kubernetes patterns: a Spring Boot API, an Uno Platform WebAssembly client, a PostgreSQL database, a Redis cache, and a complete observability stack covering all three pillars — metrics (Prometheus), logs (Loki), and traces (Tempo) — visualised in Grafana.

---

## Table of Contents

- [Architecture](#architecture)
- [Services](#services)
- [Quick Start](#quick-start)
- [Access URLs](#access-urls)
- [API Reference](#api-reference)
- [Database Schema](#database-schema)
- [Observability](#observability)
- [Kubernetes Deployment](#kubernetes-deployment)
- [Project Structure](#project-structure)

---

## Architecture

```
                        ┌─────────────────────────────────────────────────────┐
                        │                  Docker Network                     │
                        │                                                     │
  Browser               │  ┌──────────────┐   /api/*                         │
  ──────────────────────┼─►│  web-client  ├─────────────────────────────►────┼──┐
  :80 (Uno WASM + nginx)│  │  nginx :80   │                                  │  │
                        │  └──────────────┘                                  │  ▼
                        │                                                     │  ┌─────────────────┐
                        │                                                     │  │   api :8282     │
                        │                                                     │  │  Spring Boot    │
                        │                                                     │  │  Java 21 + OTel │
                        │                                                     │  └──┬──────┬───────┘
                        │                                                     │     │      │
                        │                                                     │ JPA/│   Redis│
                        │                                                     │     ▼      ▼
                        │                                                     │  ┌──────┐ ┌──────────────┐
                        │                                                     │  │  pg  │ │ redis :6379  │
                        │  ┌─────────────────────────────────────────────┐   │  │:5432 │ │  primes cache│
                        │  │              Observability Stack             │   │  └──┬───┘ └──────────────┘
                        │  │                                             │   │     │
                        │  │  prometheus :9090 ◄─── scrapes ─────────────┼───┘  ┌──▼──────────────┐
                        │  │       │                                     │      │postgres-exporter│
                        │  │       │ metrics + span-metrics              │      │    :9187        │
                        │  │       ▼                                     │      └─────────────────┘
                        │  │  grafana :3000 ◄────────────────────────────┼── traces ── tempo :3200
                        │  │       ▲                                     │                 ▲
                        │  │       │ logs                                │  OTLP :4317 ───┘
                        │  │  loki :3100 ◄── promtail ───────────────────┼── Docker socket
                        │  └─────────────────────────────────────────────┘
                        └─────────────────────────────────────────────────────┘
```

---

## Services

| Container                         | Image                       | Port       | Purpose                                         |
| --------------------------------- | --------------------------- | ---------- | ----------------------------------------------- |
| `internet-bank-postgres`          | `postgres:17-alpine`        | 5432       | Primary database                                |
| `internet-bank-redis`             | `redis:7-alpine`            | 6379       | Primes result cache (24 h TTL)                  |
| `internet-bank-api`               | _(built)_                   | 8282       | Spring Boot REST API + OTel agent               |
| `internet-bank-web`               | _(built)_                   | 80         | Uno WASM client + nginx reverse proxy           |
| `internet-bank-postgres-exporter` | `postgres-exporter:v0.16.0` | 9187       | Exposes DB metrics to Prometheus                |
| `internet-bank-prometheus`        | `prom/prometheus:v3.3.0`    | 9090       | Metrics collection & storage (15-day retention) |
| `internet-bank-loki`              | `grafana/loki:3.4.2`        | 3100       | Log aggregation                                 |
| `internet-bank-promtail`          | `grafana/promtail:3.4.2`    | —          | Ships container logs to Loki                    |
| `internet-bank-tempo`             | `grafana/tempo:2.7.1`       | 3200, 4317 | Distributed trace storage (OTLP receiver)       |
| `internet-bank-grafana`           | `grafana/grafana:11.6.1`    | 3000       | Dashboards — metrics, logs, and traces          |

### API (`api-application/`)

- **Runtime**: Java 21, Spring Boot 3.5.1
- **Virtual threads** enabled for async SSE streaming
- **Persistence**: PostgreSQL via Spring Data JPA, HikariCP connection pool (max 10)
- **Cache**: Redis (Spring Data Redis) — primes results stored with 24 h TTL; makes the API stateless and horizontally scalable
- **Metrics**: Micrometer → `micrometer-registry-prometheus`
- **Logging**: Structured JSON via `logstash-logback-encoder`
- **Tracing**: OpenTelemetry Java agent (zero-code instrumentation) → Tempo via OTLP/gRPC `:4317`
- **Build**: Three-stage Dockerfile — Maven build, OTel agent download, JRE runtime

### Web Client (`multi-client/`)

- **Framework**: Uno Platform targeting `net10.0-browserwasm`
- **Pattern**: MVVM — `MainPageViewModel` with `INotifyPropertyChanged`, async commands
- **Features**: factorial, fibonacci, streaming primes (SSE), daily mathematician
- **Served by**: nginx with gzip compression for WASM assets; SSE proxy unbuffered
- **API base URL**: resolved at runtime from `window.location` — works behind any reverse proxy

---

## Quick Start

```bash
# Clone and start everything
git clone https://github.com/tobese/internet-bank-case-study
cd internet-bank-case-study/new-bank

docker compose up -d
```

On first run Docker builds the API and web-client images (≈2–3 min). Subsequent starts are instant.

To rebuild a single service after code changes:

```bash
docker compose up -d --build api
docker compose up -d --build web-client
```

To stop everything:

```bash
docker compose down
```

---

## Access URLs

| Service                  | URL                                       | Credentials       |
| ------------------------ | ----------------------------------------- | ----------------- |
| Web application          | http://localhost                          | —                 |
| REST API                 | http://localhost:8282/api                 | —                 |
| API health               | http://localhost:8282/actuator/health     | —                 |
| Prometheus metrics (raw) | http://localhost:8282/actuator/prometheus | —                 |
| Prometheus UI            | http://localhost:9090                     | —                 |
| Grafana                  | http://localhost:3000                     | `admin` / `admin` |
| Loki (API)               | http://localhost:3100                     | —                 |
| Tempo (API)              | http://localhost:3200                     | —                 |

---

## API Reference

All endpoints are under the `/api` prefix.

### Factorial

```
GET /api/fac/{n}
```

Returns `n!` as an arbitrary-precision integer.

**Example**:

```bash
curl http://localhost:8282/api/fac/20
# {"input":20,"result":"2432902008176640000"}
```

### Fibonacci

```
GET /api/fib/{n}
```

Returns the _n_-th Fibonacci number.

**Example**:

```bash
curl http://localhost:8282/api/fib/50
# {"input":50,"result":"12586269025"}
```

### Primes (streaming)

```
GET /api/primes/sieve/{limit}
Content-Type: text/event-stream
```

Streams all prime numbers up to `limit` (2–10 000) using the Sieve of Eratosthenes via Server-Sent Events. Results are cached in Redis with a 24 h TTL; subsequent requests for the same limit stream directly from cache.

**Example**:

```bash
curl http://localhost:8282/api/primes/sieve/50
# data: 2
# data: 3
# data: 5
# ...
```

### Mathematician of the Day

```
GET /api/mathematician/daily    — deterministic daily pick
GET /api/mathematician/random   — random pick
GET /api/mathematician/{id}     — by ID
GET /api/mathematicians         — full list
```

**Example response**:

```json
{
  "id": 8,
  "name": "Alan Turing",
  "description": "British mathematician who founded computer science...",
  "era": "1912-1954"
}
```

---

## Database Schema

The database (`internet_bank`) is initialised by `init-db.sql` on first start via the Postgres `docker-entrypoint-initdb.d` mechanism.

```sql
mathematicians   -- name, description, era  (seeded with 10 famous mathematicians)
calculations     -- operation, operand, result, created_at
transactions     -- transaction_type, amount, result, created_at
audit_log        -- event_type, details (JSONB), created_at
```

Indexes are created on all `created_at` columns and `mathematicians.name`.

---

## Observability

### Metrics (Prometheus + Grafana)

Prometheus scrapes two targets every 15 seconds:

| Target                         | Job               | Metrics                                                   |
| ------------------------------ | ----------------- | --------------------------------------------------------- |
| `api:8282/actuator/prometheus` | `spring-boot-api` | JVM, HTTP, DB pool, custom math metrics                   |
| `postgres-exporter:9187`       | `postgres`        | PostgreSQL stats (connections, transactions, replication) |

**Custom application metrics** (math operations):

| Metric                     | Type                 | Tags                                       | Description                               |
| -------------------------- | -------------------- | ------------------------------------------ | ----------------------------------------- |
| `math_requests_total`      | Counter              | `operation` (factorial, fibonacci, primes) | Total requests per operation              |
| `math_calculation_seconds` | Timer                | `operation`, `n`                           | Execution time per call                   |
| `math_primes_duration`     | Timer                | —                                          | Sieve compute time (async, on completion) |
| `math_primes_count`        | Distribution summary | —                                          | Number of primes found per request        |
| `math_primes_cache_total`  | Counter              | `result` (hit, miss)                       | Primes cache effectiveness                |

**Useful PromQL queries** (run in Grafana Explore or Prometheus UI):

```promql
# Request rate per operation
rate(math_requests_total[1m])

# Average calculation latency
rate(math_calculation_seconds_sum[1m]) / rate(math_calculation_seconds_count[1m])

# 95th-percentile latency
histogram_quantile(0.95, rate(math_calculation_seconds_bucket[1m]))

# Primes cache hit ratio
rate(math_primes_cache_total{result="hit"}[1m]) / rate(math_primes_cache_total[1m])
```

### Logs (Promtail → Loki → Grafana)

The API emits structured JSON logs (via `logstash-logback-encoder`):

```json
{
  "@timestamp": "2026-04-05T18:41:16.519Z",
  "level": "INFO",
  "logger_name": "o.a.c.http11.Http11NioProtocol",
  "message": "Tomcat started on port 8282"
}
```

Promtail discovers all running containers via the Docker socket and ships their logs to Loki. In Grafana → Explore → Loki, filter by container:

```logql
{container="internet-bank-api"} | json | level="ERROR"
{container="internet-bank-api"} | json | line_format "{{.message}}"
```

Grafana is pre-provisioned with Prometheus, Loki, and Tempo datasources — no manual setup needed.

### Traces (OpenTelemetry → Tempo → Grafana)

The OTel Java agent is baked into the API image and activated via `JAVA_TOOL_OPTIONS`. It instruments every HTTP request, JDBC call, and Redis operation automatically — no application code changes needed.

| Setting          | Value                                 |
| ---------------- | ------------------------------------- |
| Exporter         | OTLP/gRPC → `tempo:4317`              |
| Service name     | `internet-bank-api`                   |
| Metrics exporter | disabled (Micrometer handles metrics) |
| Logs exporter    | disabled (Loki handles logs)          |

Tempo also runs a **metrics generator** that derives `service_graph` and `span_metrics` series and remote-writes them to Prometheus, so you can query span-level latency histograms in Grafana without any additional setup.

**In Grafana → Explore → Tempo** you can:

- Search traces by service, operation name, duration, or status
- Click a trace to see the full waterfall (HTTP handler → JDBC queries → Redis calls)
- Jump from a trace directly to the correlated Loki log lines via the Tempo→Loki link
- Jump from a Prometheus exemplar directly to the related trace

**Useful TraceQL queries**:

```
{ .service.name = "internet-bank-api" && duration > 100ms }
{ span.db.system = "postgresql" }
{ span.db.system = "redis" }
{ rootName =~ "GET /api/primes.*" }
```

---

## Kubernetes Deployment

Production-grade manifests live in `k8s/`, managed with Kustomize.

| Manifest                     | What it does                                          |
| ---------------------------- | ----------------------------------------------------- |
| `namespace.yaml`             | Dedicated `internet-bank` namespace                   |
| `postgres-statefulset.yaml`  | Postgres with persistent volume                       |
| `api-deployment.yaml`        | API deployment (2 replicas by default)                |
| `api-hpa.yaml`               | HorizontalPodAutoscaler — scales on CPU               |
| `api-pdb.yaml`               | PodDisruptionBudget — keeps min 1 pod during rollouts |
| `web-client-deployment.yaml` | Nginx + WASM client                                   |
| `web-client-ingress.yaml`    | Ingress controller rules                              |
| `network-policy.yaml`        | Restricts inter-pod traffic                           |
| `prometheus-deployment.yaml` | In-cluster Prometheus                                 |

The `deploy.sh` script handles building, optional registry push, and applying manifests:

```bash
./deploy.sh                   # build + deploy (minikube / kind)
./deploy.sh -p                # ... then port-forward to localhost:8080
./deploy.sh -r ghcr.io/myorg  # push to a remote registry first
./deploy.sh -s                # skip builds, re-apply manifests only
./deploy.sh -h                # full help
```

---

## Project Structure

```
new-bank/
├── api-application/        # Spring Boot API (Java 21)
│   ├── src/main/java/      # Controllers, services, models
│   ├── src/main/resources/ # application.properties, logback-spring.xml
│   └── Dockerfile          # Multi-stage Maven + JRE build
├── multi-client/           # Uno Platform WASM client
│   ├── InternetBankCalculator/
│   │   └── InternetBankCalculator/
│   │       ├── ViewModels/ # MVVM view models
│   │       ├── Services/   # MathApiService (HTTP + SSE)
│   │       └── Models/     # API response DTOs
│   ├── nginx.conf          # Reverse proxy + SSE + gzip config
│   └── Dockerfile          # dotnet publish + nginx
├── k8s/                    # Kubernetes manifests (Kustomize)
├── loki/                   # Loki & Promtail config
├── tempo/                  # Grafana Tempo config
├── grafana/                # Grafana provisioning (datasources)
├── prometheus/             # Prometheus scrape config
├── docker-compose.yml      # Full local stack (10 services)
├── init-db.sql             # DB schema + seed data
└── deploy.sh               # Build + K8s deploy script
```
