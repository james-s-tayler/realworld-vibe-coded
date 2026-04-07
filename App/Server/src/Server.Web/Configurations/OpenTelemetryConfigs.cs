using Azure.Monitor.OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Server.Web.Configurations;

public static class OpenTelemetryConfigs
{
  private const string ServiceName = "Conduit";

  public static IServiceCollection AddOpenTelemetryConfigs(
    this IServiceCollection services,
    IConfiguration configuration,
    IWebHostEnvironment environment)
  {
    var otlpEndpoint = configuration["OpenTelemetry:OtlpEndpoint"];
    var appInsightsConnectionString = configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]
      ?? Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING");

    var resourceBuilder = ResourceBuilder.CreateDefault()
      .AddService(ServiceName, serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0");

    services.AddOpenTelemetry()
      .WithTracing(tracing =>
      {
        tracing.SetResourceBuilder(resourceBuilder)
          .AddAspNetCoreInstrumentation(options =>
          {
            options.Filter = context =>
              !context.Request.Path.StartsWithSegments("/health") &&
              !context.Request.Path.StartsWithSegments("/swagger") &&
              !context.Request.Path.StartsWithSegments("/metrics");
          })
          .AddHttpClientInstrumentation()
          .AddEntityFrameworkCoreInstrumentation()
          .AddSource("Conduit.MediatR");

        if (!string.IsNullOrEmpty(otlpEndpoint))
        {
          tracing.AddOtlpExporter(options =>
          {
            options.Endpoint = new Uri(otlpEndpoint);
          });
        }

        if (!string.IsNullOrEmpty(appInsightsConnectionString))
        {
          tracing.AddAzureMonitorTraceExporter(options =>
          {
            options.ConnectionString = appInsightsConnectionString;
          });
        }
      })
      .WithMetrics(metrics =>
      {
        metrics.SetResourceBuilder(resourceBuilder)
          .AddAspNetCoreInstrumentation()
          .AddHttpClientInstrumentation()
          .AddRuntimeInstrumentation()
          .AddPrometheusExporter();

        if (!string.IsNullOrEmpty(appInsightsConnectionString))
        {
          metrics.AddAzureMonitorMetricExporter(options =>
          {
            options.ConnectionString = appInsightsConnectionString;
          });
        }
      });

    return services;
  }
}
