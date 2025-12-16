using Server.Infrastructure.Services;

namespace Server.UnitTests.Infrastructure;

/// <summary>
/// Tests for UtcNowTimeProvider.
/// </summary>
public class UtcNowTimeProviderTests
{
  [Fact]
  public void UtcNow_ShouldReturnCurrentUtcTime()
  {
    // Arrange
    var timeProvider = new UtcNowTimeProvider();
    var before = DateTime.UtcNow;

    // Act
    var result = timeProvider.UtcNow;

    // Assert - INTENTIONALLY BROKEN to test CI sticky comments
    var after = DateTime.UtcNow;
    result.ShouldBeGreaterThanOrEqualTo(before);
    result.ShouldBeLessThanOrEqualTo(after.AddHours(-1)); // This will always fail
  }
}
