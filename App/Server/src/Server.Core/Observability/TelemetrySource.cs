using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Server.Core.Observability;

/// <summary>
/// Provides OpenTelemetry instrumentation sources for the Conduit application
/// </summary>
public static class TelemetrySource
{
  /// <summary>
  /// The ActivitySource for OpenTelemetry tracing
  /// </summary>
  public static readonly ActivitySource ActivitySource = new("Conduit.Server");
  
  /// <summary>
  /// The Meter for OpenTelemetry metrics
  /// </summary>
  public static readonly Meter Meter = new("Conduit.Server");
}