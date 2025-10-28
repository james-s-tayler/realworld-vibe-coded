using Server.SharedKernel.DomainEvents;
using Server.SharedKernel.Persistence;

namespace Server.UnitTests.SharedKernel;

/// <summary>
/// Tests for HasDomainEventsBase.
/// </summary>
public class HasDomainEventsBaseTests
{
  private class TestDomainEvent : DomainEventBase
  {
  }

  private class TestEntity : HasDomainEventsBase
  {
    public void AddEvent(DomainEventBase domainEvent)
    {
      RegisterDomainEvent(domainEvent);
    }
  }

  [Fact]
  public void RegisterDomainEvent_ShouldAddEventToCollection()
  {
    // Arrange
    var entity = new TestEntity();
    var domainEvent = new TestDomainEvent();

    // Act
    entity.AddEvent(domainEvent);

    // Assert
    entity.DomainEvents.Count.ShouldBe(1);
    entity.DomainEvents.First().ShouldBe(domainEvent);
  }

  [Fact]
  public void ClearDomainEvents_ShouldRemoveAllEvents()
  {
    // Arrange
    var entity = new TestEntity();
    entity.AddEvent(new TestDomainEvent());
    entity.AddEvent(new TestDomainEvent());

    // Act
    entity.ClearDomainEvents();

    // Assert
    entity.DomainEvents.Count.ShouldBe(0);
  }
}
