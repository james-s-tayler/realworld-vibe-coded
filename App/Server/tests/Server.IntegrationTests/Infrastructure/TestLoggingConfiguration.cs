using Serilog;

namespace Server.IntegrationTests.Infrastructure;

/// <summary>
/// Helper class for configuring Serilog logging in tests.
/// Writes logs to both console and to the filesystem.
/// </summary>
public static class TestLoggingConfiguration
{
  /// <summary>
  /// Creates a Serilog logger that writes to both console and file system.
  /// </summary>
  /// <param name="testProjectName">Name of the test project (e.g., "Server.IntegrationTests")</param>
  /// <param name="testClassName">Name of the test class for log file naming</param>
  /// <returns>Configured Serilog logger</returns>
  public static ILogger CreateLogger(string testProjectName, string? testClassName = null)
  {
    var logDirectory = Environment.GetEnvironmentVariable("XUNIT_LOG_DIRECTORY");

    if (string.IsNullOrEmpty(logDirectory))
    {
      // Fallback to default location if environment variable is not set
      var repositoryRoot = FindRepositoryRoot();
      logDirectory = Path.Combine(repositoryRoot, "Logs", "Server", "tests", testProjectName, "xUnit");
    }

    Directory.CreateDirectory(logDirectory);

    var logFileName = string.IsNullOrEmpty(testClassName)
      ? "test-run.log"
      : $"{testClassName}.log";
    var logFilePath = Path.Combine(logDirectory, logFileName);

    var loggerConfig = new LoggerConfiguration()
      .MinimumLevel.Debug()
      .Enrich.FromLogContext()
      .Enrich.WithProperty("TestProject", testProjectName)
      .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}")
      .WriteTo.File(
        logFilePath,
        outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}",
        rollingInterval: RollingInterval.Day,
        shared: true,
        flushToDiskInterval: TimeSpan.FromSeconds(1));

    return loggerConfig.CreateLogger();
  }

  private static string FindRepositoryRoot()
  {
    var directory = Directory.GetCurrentDirectory();
    while (directory != null)
    {
      if (Directory.Exists(Path.Combine(directory, ".git")))
      {
        return directory;
      }
      directory = Directory.GetParent(directory)?.FullName;
    }
    return Directory.GetCurrentDirectory();
  }
}
