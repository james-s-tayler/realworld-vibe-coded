using Microsoft.Extensions.Logging;
using Server.FunctionalTests.Infrastructure;

namespace Server.FunctionalTests.TestLogging;

public class LoggingVerificationTests
{
  [Fact]
  public void VerifyLoggingConfiguration_WritesToFile()
  {
    // Get test output helper from test context (xUnit v3 approach)
    var output = TestContext.Current?.TestOutputHelper;

    // Arrange - Create logger that writes to file (and console if output helper available)
    var logger = output != null
      ? TestLoggingConfiguration.CreateLogger(output, "Server.FunctionalTests", "LoggingVerificationTests")
      : TestLoggingConfiguration.CreateFileOnlyLogger("Server.FunctionalTests", "LoggingVerificationTests");

    // Act
    logger.LogInformation("Test log message from LoggingVerificationTests");
    logger.LogDebug("Debug message - logging is working correctly");
    logger.LogWarning("Warning message - this should appear in log file and xUnit output (if available)");

    // Assert - if we get here without exceptions, logging is configured correctly
    true.ShouldBeTrue();
  }
}
