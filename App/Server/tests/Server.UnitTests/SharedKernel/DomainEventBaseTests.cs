using Server.SharedKernel.Persistence;

namespace Server.UnitTests.SharedKernel;

/// <summary>
/// Tests for DomainEventBase.
/// </summary>
public class DomainEventBaseTests
{
  private class TestDomainEvent : DomainEventBase
  {
  }

  [Fact]
  public void DomainEventBase_ShouldSetDateOccurred()
  {
    // Arrange
    var before = DateTime.UtcNow;

    // Act
    var domainEvent = new TestDomainEvent();

    // Assert
    var after = DateTime.UtcNow;
    domainEvent.DateOccurred.ShouldBeGreaterThanOrEqualTo(before);
    domainEvent.DateOccurred.ShouldBeLessThanOrEqualTo(after);
  }
}
