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

    // Assert
    var after = DateTime.UtcNow;
    result.ShouldBeGreaterThanOrEqualTo(before);
    result.ShouldBeLessThanOrEqualTo(after);
  }
}
