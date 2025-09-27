# realworld-vibe-coded
An attempt at implementing gothinkster/realworld purely through vibe-coding

## Specifications
- [Endpoints](https://docs.realworld.show/specifications/backend/endpoints/)

## Observability Features

This application includes comprehensive OpenTelemetry instrumentation for monitoring, tracing, and metrics collection.

### 🔍 Tracing
- **Distributed tracing** for all HTTP requests and business operations
- **Custom activities** in key handlers (CreateArticleHandler, LoginUserHandler, GetCurrentUserHandler)
- **Detailed tags** including user IDs, article slugs, operation results, and business context
- **Error tracking** with activity status and error messages
- **Database queries** traced through Entity Framework Core instrumentation

### 📊 Metrics
- **HTTP request metrics** (duration, status codes, endpoints)
- **Custom command metrics** (execution count, duration, success/error rates)
- **Entity Framework metrics** (query execution times, connection pools)
- **System metrics** (memory, CPU, GC)
- **Prometheus-compatible** metrics endpoint at `/metrics`

### 📝 Structured Logging
- **Serilog integration** with OpenTelemetry
- **Correlation IDs** linking logs to traces
- **Structured properties** for filtering and analysis
- **Console output** in development, extensible for production

### 🚀 Getting Started with Observability

#### Metrics Endpoint
The application exposes Prometheus-compatible metrics at:
```
GET /metrics
```

#### Local Development
Telemetry is configured to output to the console by default. You'll see:
- Trace spans with detailed operation context
- Structured log entries with correlation
- Metrics exported to console (can be disabled)

#### Production Setup
For production monitoring, configure external exporters:

```csharp
// Add to OpenTelemetryConfigs.cs
.WithTracing(tracing => tracing
    .AddJaegerExporter() // For trace collection
    .AddOtlpExporter())  // For OTLP-compatible backends

.WithMetrics(metrics => metrics
    .AddPrometheusExporter() // Already configured
    .AddOtlpExporter())      // For metrics backends
```

#### Key Instrumented Operations
- ✅ **Article Creation** - Full lifecycle tracing with validation, slug generation, tag handling
- ✅ **User Authentication** - Login attempts with success/failure tracking
- ✅ **Current User Retrieval** - User session validation and token refresh
- ✅ **Command Processing** - All MediatR commands via OpenTelemetryCommandLogger
- ✅ **HTTP Requests** - All ASP.NET Core endpoints
- ✅ **Database Queries** - Entity Framework Core operations

#### Monitoring Dashboards
The metrics can be visualized using:
- **Prometheus + Grafana** for metrics dashboards
- **Jaeger or Zipkin** for distributed tracing
- **OTEL-compatible APM tools** (Datadog, New Relic, Azure Monitor)

### Implementation Details
- **ActivitySource**: `Conduit.Server` - used for creating custom spans
- **Meter**: `Conduit.Server` - used for custom metrics
- **Service Name**: `Conduit.Server` - identifies the service in observability tools
- **Architecture**: Clean separation in `Server.Core.Observability.TelemetrySource`

## Notes on Vibe Coding
- [Copilot Docs](https://code.visualstudio.com/docs/copilot/overview)
- [Awesome Copilot](https://github.com/github/awesome-copilot/tree/main)
- [Use prompt files in VS Code](https://code.visualstudio.com/docs/copilot/customization/prompt-files)
- [Adding repository custom instructions for GitHub Copilot](https://docs.github.com/en/copilot/how-tos/configure-custom-instructions/add-repository-instructions?tool=jetbrains)

## Notes on MCP
- [MCP Servers](https://github.com/modelcontextprotocol/servers/tree/main)
- [GitHub MCP Server](https://github.com/github/github-mcp-server)
- [Hyperbrowser](https://github.com/hyperbrowserai/mcp)
- [Kintone](https://github.com/kintone/mcp-server?tab=readme-ov-file)
- [Microsoft Learn MCP Server](https://github.com/microsoftdocs/mcp)
- [Playwright MCP](https://github.com/microsoft/playwright-mcp)
- [Postman MCP Server](https://github.com/postmanlabs/postman-mcp-server)
- [Azure MCP](https://github.com/Azure-Samples/mcp)
- [MCP Devcontainers](https://github.com/AI-QL/mcp-devcontainers)
- [Excel MCP Server](https://github.com/haris-musa/excel-mcp-server)
- [Excel to JSON](https://github.com/he-yang/excel-to-json-mcp)
- [Figma MCP Server](https://github.com/paulvandermeijs/figma-mcp)
- [n8n MCP Server](https://github.com/leonardsellem/n8n-mcp-server)
- [Unleash Feature Toggle MCP](https://github.com/cuongtl1992/unleash-mcp)
- [Workflowy](https://github.com/danield137/mcp-workflowy)
