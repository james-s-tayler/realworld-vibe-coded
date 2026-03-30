# Observability Stack

## Overview

The application uses OpenTelemetry for distributed tracing and metrics, with Serilog for structured logging. In local development, telemetry flows to a Grafana + Tempo + Prometheus stack. In production, the same instrumentation exports to Azure Application Insights via environment variable — zero code changes.

```
LOCAL DEV:
.NET App
├── Traces  → OTLP gRPC push  → Tempo       → Grafana
├── Metrics → /metrics scrape  → Prometheus  → Grafana
└── Logs    → Serilog          → Seq (unchanged)

PRODUCTION (Azure):
.NET App
├── Traces  ─┐
├── Metrics  ├→ Azure Monitor Exporter → Application Insights
└── Logs     ─┘   (via APPLICATIONINSIGHTS_CONNECTION_STRING env var)
```

## Local URLs

| Service | URL | Purpose |
|:--------|:----|:--------|
| **Grafana** | [http://localhost:3000](http://localhost:3000) | Dashboards, trace explorer, metrics explorer |
| **Seq** | [http://localhost:5341](http://localhost:5341) | Structured log search (Serilog) |
| **Prometheus** | [http://localhost:9090](http://localhost:9090) | Metrics query UI, target health |
| **Tempo API** | [http://localhost:3200](http://localhost:3200) | Trace storage API (queried by Grafana) |
| **App Metrics** | [http://localhost:5000/metrics](http://localhost:5000/metrics) | Prometheus scrape endpoint exposed by the app |

## Audit Logs (Audit.NET)

Audit.NET captures entity-level change tracking for all EF Core operations, writing JSON audit logs to disk:

- **Location**: `Logs/Server.Web/Audit.NET/` (configurable via `Audit:LogsPath` in appsettings)
- **What's captured**: Insert, Update, Delete operations with before/after values for every entity property
- **Format**: JSON files with timestamps, entity type, primary key, changed properties, old/new values
- **Identity events**: Authentication events (login, logout, failed attempts) are also audited

See `Docs/AUDIT.md` for full details on the audit log format and configuration.

## What Gets Instrumented

### Traces (Tempo → Grafana)

- **ASP.NET Core** — every HTTP request (excluding `/health`, `/swagger`, `/metrics`)
- **HttpClient** — outgoing HTTP calls
- **Entity Framework Core** — database queries and commands
- **MediatR** — every command and query via `TracingBehavior`
  - Span name: `MediatR Command: CreateArticleCommand` or `MediatR Query: GetProfileQuery`
  - Tags: `mediatr.request.name`, `mediatr.request.kind` (Command/Query), `mediatr.result.status`
  - Error status set on non-success results

### Metrics (Prometheus → Grafana)

- **ASP.NET Core** — `http.server.request.duration`, request counts by status code, route
- **HttpClient** — outgoing request duration and counts
- **.NET Runtime** — GC collections, heap size, threadpool queue length, exception count
- **SQL Server** — connections, batch requests/sec, page life expectancy, buffer cache hit ratio, deadlocks (via `sql-exporter` container)

### Logs (Serilog → Seq)

- Structured logging with `TraceId` enrichment for cross-correlation
- Console output includes `{TraceId}` so you can search Grafana traces by the ID shown in terminal
- Seq shows `TraceId` as a searchable property on every log event

## Cross-Correlation

Serilog 4.x automatically enriches log events with `TraceId` and `SpanId` from `Activity.Current` via the `FromLogContext` enricher. This means:

1. See a log entry in Seq with a `TraceId`
2. Copy that `TraceId` into Grafana → Explore → Tempo
3. See the full distributed trace with ASP.NET Core, EF Core, and MediatR spans

## Architecture

### TracingBehavior

`App/Server/src/Server.SharedKernel/MediatR/TracingBehavior.cs`

A MediatR pipeline behavior that wraps every command/query in an OpenTelemetry span. Uses only `System.Diagnostics.ActivitySource` — no OTel SDK dependency in SharedKernel. Registered as the first pipeline behavior:

```
TracingBehavior → LoggingBehavior → TransactionBehavior → ExceptionHandlingBehavior
```

### OpenTelemetryConfigs

`App/Server/src/Server.Web/Configurations/OpenTelemetryConfigs.cs`

Extension method that configures:
- **Tracing**: ASP.NET Core + HttpClient + EF Core + MediatR instrumentation, OTLP exporter
- **Metrics**: ASP.NET Core + HttpClient + Runtime instrumentation, Prometheus exporter
- **Azure Monitor**: Conditionally added when `APPLICATIONINSIGHTS_CONNECTION_STRING` env var is set

### Docker Compose Services

Defined in `Task/LocalDev/docker-compose.dev-deps.yml`:

| Container | Image | Purpose |
|:----------|:------|:--------|
| `tempo` | `grafana/tempo` | Trace storage, receives OTLP gRPC on port 4317 |
| `prometheus` | `prom/prometheus` | Metrics storage, scrapes app `/metrics` and SQL exporter |
| `sql-exporter` | `awaragi/prometheus-mssql-exporter` | Exports SQL Server DMV metrics to Prometheus |
| `grafana` | `grafana/grafana` | Visualization — auto-provisioned with Tempo + Prometheus datasources |

Config files in `Task/LocalDev/config/`:
- `tempo.yaml` — OTLP receiver + local block storage
- `prometheus.yml` — scrape targets (host app, Docker app, SQL Server)
- `grafana/datasources.yaml` — auto-provisioned datasources

## Configuration

### Local Development (`appsettings.json`)

```json
{
  "OpenTelemetry": {
    "OtlpEndpoint": "http://localhost:4317"
  }
}
```

### Testing (`appsettings.Testing.json`)

```json
{
  "OpenTelemetry": {
    "OtlpEndpoint": ""
  }
}
```

Empty endpoint disables OTLP export — traces and metrics are still collected in-process but not exported.

### Docker (`docker-compose.publish.yml`)

```yaml
OpenTelemetry__OtlpEndpoint: "http://tempo:4317"
```

### Production (Azure Application Insights)

Set the `APPLICATIONINSIGHTS_CONNECTION_STRING` environment variable. The code auto-detects it and adds Azure Monitor trace and metric exporters. OTLP and Prometheus exporters still work alongside if configured.

## Using Grafana

### Viewing Traces

1. Open [http://localhost:3000](http://localhost:3000)
2. Go to **Explore** (compass icon in sidebar)
3. Select **Tempo** datasource
4. Choose **Search** tab and filter by service name `Conduit`
5. Click a trace to see the span waterfall (ASP.NET Core → MediatR → EF Core)

### Viewing Metrics

1. Open [http://localhost:3000](http://localhost:3000)
2. Go to **Explore** (compass icon in sidebar)
3. Select **Prometheus** datasource
4. Query metrics like:
   - `http_server_request_duration_seconds_bucket` — request latency histogram
   - `process_runtime_dotnet_gc_collections_total` — GC collection counts
   - `mssql_connections` — SQL Server connection count

### SQL Server Metrics

The `sql-exporter` container exposes SQL Server DMV metrics including:
- Active connections
- Batch requests per second
- Page life expectancy
- Buffer cache hit ratio
- Deadlock count
