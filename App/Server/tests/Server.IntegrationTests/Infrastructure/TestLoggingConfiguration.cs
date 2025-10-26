using Microsoft.Extensions.Logging;
using Serilog;

namespace Server.IntegrationTests.Infrastructure;

/// <summary>
/// Helper class for configuring test logging with xUnit ITestOutputHelper integration.
/// Writes logs to both xUnit test output and to the filesystem.
/// Supports both xUnit v2 and v3.
/// </summary>
public static class TestLoggingConfiguration
{
  /// <summary>
  /// Creates a Microsoft.Extensions.Logging.ILoggerFactory that writes to both xUnit output and file system.
  /// </summary>
  /// <param name="testOutputHelper">xUnit test output helper for console output (supports both v2 and v3)</param>
  /// <param name="testProjectName">Name of the test project (e.g., "Server.IntegrationTests")</param>
  /// <param name="testClassName">Name of the test class for log file naming</param>
  /// <returns>Configured ILoggerFactory</returns>
  public static ILoggerFactory CreateLoggerFactory(object testOutputHelper, string testProjectName, string? testClassName = null)
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

    // Configure Serilog to write to both xUnit output and file
    var serilogLogger = new LoggerConfiguration()
      .MinimumLevel.Debug()
      .Enrich.FromLogContext()
      .Enrich.WithProperty("TestProject", testProjectName)
      .WriteTo.Sink(new TestOutputSink(testOutputHelper))
      .WriteTo.File(
        logFilePath,
        outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}",
        rollingInterval: RollingInterval.Day,
        shared: true,
        flushToDiskInterval: TimeSpan.FromSeconds(1))
      .CreateLogger();

    // Create a logger factory using Serilog
    var loggerFactory = LoggerFactory.Create(builder =>
    {
      builder.AddSerilog(serilogLogger, dispose: true);
      builder.SetMinimumLevel(LogLevel.Debug);
    });

    return loggerFactory;
  }

  /// <summary>
  /// Creates a Microsoft.Extensions.Logging.ILoggerFactory that writes only to file system (no console output).
  /// </summary>
  /// <param name="testProjectName">Name of the test project (e.g., "Server.IntegrationTests")</param>
  /// <param name="testClassName">Name of the test class for log file naming</param>
  /// <returns>Configured ILoggerFactory</returns>
  public static ILoggerFactory CreateFileOnlyLoggerFactory(string testProjectName, string? testClassName = null)
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

    // Configure Serilog to write only to file
    var serilogLogger = new LoggerConfiguration()
      .MinimumLevel.Debug()
      .Enrich.FromLogContext()
      .Enrich.WithProperty("TestProject", testProjectName)
      .WriteTo.File(
        logFilePath,
        outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}",
        rollingInterval: RollingInterval.Day,
        shared: true,
        flushToDiskInterval: TimeSpan.FromSeconds(1))
      .CreateLogger();

    // Create a logger factory using Serilog
    var loggerFactory = LoggerFactory.Create(builder =>
    {
      builder.AddSerilog(serilogLogger, dispose: true);
      builder.SetMinimumLevel(LogLevel.Debug);
    });

    return loggerFactory;
  }

  /// <summary>
  /// Creates a Microsoft.Extensions.Logging.ILogger that writes to both xUnit output and file system.
  /// </summary>
  /// <param name="testOutputHelper">xUnit test output helper for console output (supports both v2 and v3)</param>
  /// <param name="testProjectName">Name of the test project (e.g., "Server.IntegrationTests")</param>
  /// <param name="testClassName">Name of the test class for log file naming</param>
  /// <returns>Configured ILogger</returns>
  public static Microsoft.Extensions.Logging.ILogger CreateLogger(object testOutputHelper, string testProjectName, string? testClassName = null)
  {
    var loggerFactory = CreateLoggerFactory(testOutputHelper, testProjectName, testClassName);
    return loggerFactory.CreateLogger(testClassName ?? "Test");
  }

  /// <summary>
  /// Creates a Microsoft.Extensions.Logging.ILogger that writes only to file system (no console output).
  /// </summary>
  /// <param name="testProjectName">Name of the test project (e.g., "Server.IntegrationTests")</param>
  /// <param name="testClassName">Name of the test class for log file naming</param>
  /// <returns>Configured ILogger</returns>
  public static Microsoft.Extensions.Logging.ILogger CreateFileOnlyLogger(string testProjectName, string? testClassName = null)
  {
    var loggerFactory = CreateFileOnlyLoggerFactory(testProjectName, testClassName);
    return loggerFactory.CreateLogger(testClassName ?? "Test");
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
