# Test Logging Infrastructure

This document explains the test logging infrastructure for xUnit tests in this project.

## Overview

All xUnit tests are configured to write logs to both the console and the filesystem. This makes it easier to debug test failures and analyze test behavior.

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

To add logging to your tests, use the `TestLoggingConfiguration` helper class:

```csharp
using Server.FunctionalTests.Infrastructure;
using Serilog;

public class MyTests
{
  [Fact]
  public void MyTest()
  {
    // Create a logger for this test
    var logger = TestLoggingConfiguration.CreateLogger(
      "Server.FunctionalTests", 
      "MyTests");
    
    // Log messages
    logger.Information("Test started");
    logger.Debug("Debug information");
    logger.Warning("Warning message");
    
    // Your test logic here
    
    // Cleanup (important!)
    Log.CloseAndFlush();
  }
}
```

## Console Output

Logs are also written to the console during test execution, making them visible in:
- Visual Studio Test Explorer
- `dotnet test` command output
- CI/CD pipeline logs
- Nuke build output

## Configuration

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

1. **Check the log directory exists**: The logging infrastructure automatically creates directories, but verify permissions.
2. **Check the environment variable**: The `XUNIT_LOG_DIRECTORY` should be set during test runs via the Nuke build.
3. **Verify Serilog packages**: Ensure `Serilog.Sinks.Console` and `Serilog.Sinks.File` are installed.
4. **Check test cleanup**: Make sure `Log.CloseAndFlush()` is called to flush logs to disk.

## Log Retention

Logs are organized by date and are not automatically cleaned up. You may want to periodically clean old logs:

```bash
# Remove logs older than 30 days
find Logs/Server/tests -name "*.log" -mtime +30 -delete
```

## .gitignore

The `Logs/` directory is excluded from version control, so test logs are not committed to the repository.
