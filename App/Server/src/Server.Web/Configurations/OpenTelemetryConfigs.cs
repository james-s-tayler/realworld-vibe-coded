using System.Diagnostics;
using System.Diagnostics.Metrics;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Server.Core.Observability;

namespace Server.Web.Configurations;

public static class OpenTelemetryConfigs
{
  public static WebApplicationBuilder AddOpenTelemetryConfigs(this WebApplicationBuilder builder)
  {
    var serviceName = "Conduit.Server";
    var serviceVersion = typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0";

    builder.Services.AddOpenTelemetry()
      .ConfigureResource(resource => resource
        .AddService(serviceName: serviceName, serviceVersion: serviceVersion)
        .AddAttributes(new Dictionary<string, object>
        {
          ["service.namespace"] = "Conduit",
          ["service.instance.id"] = Environment.MachineName
        }))
      .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation(options =>
        {
          options.RecordException = true;
          options.Filter = httpContext =>
          {
            // Skip health checks and metrics endpoints from tracing
            var path = httpContext.Request.Path.Value;
            return !string.IsNullOrEmpty(path) &&
                   !path.StartsWith("/health", StringComparison.OrdinalIgnoreCase) &&
                   !path.StartsWith("/metrics", StringComparison.OrdinalIgnoreCase);
          };
        })
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation(options =>
        {
          options.SetDbStatementForText = true;
          options.SetDbStatementForStoredProcedure = true;
        })
        .AddSource(TelemetrySource.ActivitySource.Name)
        .AddConsoleExporter())
      .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddMeter(TelemetrySource.Meter.Name)
        .AddPrometheusExporter());

    // Configure OpenTelemetry logging
    builder.Logging.AddOpenTelemetry(logging => logging
      .SetResourceBuilder(ResourceBuilder.CreateDefault()
        .AddService(serviceName: serviceName, serviceVersion: serviceVersion))
      .AddConsoleExporter());

    return builder;
  }

  public static WebApplication UseOpenTelemetryConfigs(this WebApplication app)
  {
    // Add Prometheus metrics endpoint
    app.UseRouting();
    app.MapPrometheusScrapingEndpoint("/metrics");

    return app;
  }
}
