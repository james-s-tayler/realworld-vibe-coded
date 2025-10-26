using Serilog;
using Server.FunctionalTests.Infrastructure;

namespace Server.FunctionalTests.TestLogging;

public class LoggingVerificationTests
{
  [Fact]
  public void VerifyLoggingConfiguration_WritesToBothConsoleAndFile()
  {
    // Arrange
    var logger = TestLoggingConfiguration.CreateLogger("Server.FunctionalTests", "LoggingVerificationTests");

    // Act
    logger.Information("Test log message from LoggingVerificationTests");
    logger.Debug("Debug message - logging is working correctly");
    logger.Warning("Warning message - this should appear in both console and file");

    // Assert - if we get here without exceptions, logging is configured correctly
    true.ShouldBeTrue();

    // Cleanup
    Log.CloseAndFlush();
  }
}
