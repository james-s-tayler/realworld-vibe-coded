# Test Logging Infrastructure

This document explains the test logging infrastructure for xUnit tests in this project.

## Overview

All xUnit tests are configured to write logs to both xUnit test output (visible in test results) and the filesystem. This makes it easier to debug test failures and analyze test behavior using `ITestOutputHelper` and a custom Serilog sink.

## Log Locations

Test logs are written to: `Logs/Server/tests/${testProjectName}/xUnit/`

For example:
- `Logs/Server/tests/Server.FunctionalTests/xUnit/`
- `Logs/Server/tests/Server.UnitTests/xUnit/`
- `Logs/Server/tests/Server.IntegrationTests/xUnit/`
- `Logs/Server/tests/Server.SharedKernel.Result.UnitTests/xUnit/`

## Log Files

Logs are organized by date with the format: `{testClassName}YYYYMMDD.log`

Example: `LoggingVerificationTests20251026.log`

## Using Logging in Tests

To add logging to your tests, inject `ITestOutputHelper` into your test class constructor and use the `TestLoggingConfiguration` helper:

```csharp
using Microsoft.Extensions.Logging;
using Server.FunctionalTests.Infrastructure;
using Xunit.Abstractions;

public class MyTests(ITestOutputHelper output)
{
  private readonly ITestOutputHelper _output = output;

  [Fact]
  public void MyTest()
  {
    // Create a logger for this test
    var logger = TestLoggingConfiguration.CreateLogger(
      _output,
      "Server.FunctionalTests", 
      "MyTests");
    
    // Log messages - these will appear in both xUnit output and the log file
    logger.LogInformation("Test started");
    logger.LogDebug("Debug information");
    logger.LogWarning("Warning message");
    
    // Your test logic here
  }
}
```

## Console Output (xUnit Test Output)

Logs are written to xUnit's test output using `ITestOutputHelper`, making them visible in:
- Visual Studio Test Explorer
- `dotnet test` command output
- CI/CD pipeline test results
- Nuke build output

Each test will have its own section with all log messages that occurred during that test.

## Configuration

### Packages Used

- **Serilog**: Core logging framework
- **Serilog.Sinks.File**: Writes logs to the filesystem for persistent storage
- **Custom TestOutputSink**: A custom Serilog sink that writes to xUnit's `ITestOutputHelper`

### xUnit Configuration

Each test project has an `xunit.runner.json` file that enables diagnostic logging:

```json
{
  "shadowCopy": false,
  "parallelizeAssembly": false,
  "parallelizeTestCollections": false,
  "diagnosticMessages": true,
  "internalDiagnosticMessages": true
}
```

### Nuke Build Integration

The Nuke `TestServer` target automatically:
1. Creates log directories for each test project
2. Sets the `XUNIT_LOG_DIRECTORY` environment variable
3. Ensures logs are written to the correct location

## Troubleshooting

If logs are not being generated:

1. **Check ITestOutputHelper injection**: Make sure your test class constructor accepts `ITestOutputHelper` as a parameter.
2. **Check the log directory exists**: The logging infrastructure automatically creates directories, but verify permissions.
3. **Check the environment variable**: The `XUNIT_LOG_DIRECTORY` should be set during test runs via the Nuke build.
4. **Verify packages**: Ensure `Serilog` and `Serilog.Sinks.File` are installed.

## Log Retention

Logs are organized by date and are not automatically cleaned up. You may want to periodically clean old logs:

```bash
# Remove logs older than 30 days
find Logs/Server/tests -name "*.log" -mtime +30 -delete
```

## .gitignore

The `Logs/` directory is excluded from version control, so test logs are not committed to the repository.

## Advanced: Using ILoggerFactory

For more complex scenarios where you need multiple loggers or want to configure logging for a larger scope, you can use `CreateLoggerFactory`:

```csharp
public class MyTests(ITestOutputHelper output)
{
  private readonly ITestOutputHelper _output = output;

  [Fact]
  public void MyComplexTest()
  {
    // Create a logger factory
    var loggerFactory = TestLoggingConfiguration.CreateLoggerFactory(
      _output,
      "Server.FunctionalTests",
      "MyTests");
    
    // Create multiple loggers from the same factory
    var logger1 = loggerFactory.CreateLogger("Component1");
    var logger2 = loggerFactory.CreateLogger("Component2");
    
    logger1.LogInformation("Message from component 1");
    logger2.LogInformation("Message from component 2");
  }
}
```

## Implementation Details

The logging infrastructure uses a custom `TestOutputSink` class that implements Serilog's `ILogEventSink` interface to write log messages to xUnit's `ITestOutputHelper`. This approach is compatible with xUnit v3 and provides seamless integration between Serilog and xUnit's test output system.
