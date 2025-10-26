namespace Server.Web.Configurations;

public static class LoggerConfigs
{
  private const string LogFileName = "logs.json";

  public static WebApplicationBuilder AddLoggerConfigs(this WebApplicationBuilder builder)
  {

    builder.Host.UseSerilog((context, services, config) =>
    {
      config.ReadFrom.Configuration(builder.Configuration);

      // Override log file path if TEST_LOG_PATH environment variable is set
      // This allows test runs to write logs to a dedicated test log directory
      var testLogPath = Environment.GetEnvironmentVariable("TEST_LOG_PATH");
      if (!string.IsNullOrEmpty(testLogPath))
      {
        config.WriteTo.File(
          path: Path.Combine(testLogPath, LogFileName),
          rollingInterval: RollingInterval.Day,
          formatter: new Serilog.Formatting.Compact.CompactJsonFormatter());
      }
    });

    return builder;
  }
}
