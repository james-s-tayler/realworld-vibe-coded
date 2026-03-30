using Microsoft.Extensions.Logging.Abstractions;
using Server.SharedKernel.DomainEvents;
using Server.SharedKernel.Persistence;

namespace Server.UnitTests.SharedKernel;

public class MediatRDomainEventDispatcherTests
{
  private readonly IMediator _mediator;
  private readonly MediatRDomainEventDispatcher _dispatcher;

  public MediatRDomainEventDispatcherTests()
  {
    _mediator = Substitute.For<IMediator>();
    var logger = NullLogger<MediatRDomainEventDispatcher>.Instance;
    _dispatcher = new MediatRDomainEventDispatcher(_mediator, logger);
  }

  [Fact]
  public async Task DispatchAndClearEvents_WithSingleEvent_PublishesAndClears()
  {
    var entity = new TestEntity();
    entity.AddEvent(new TestDomainEvent());

    await _dispatcher.DispatchAndClearEvents([entity]);

    await _mediator.Received(1).Publish(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>());
    entity.DomainEvents.ShouldBeEmpty();
  }

  [Fact]
  public async Task DispatchAndClearEvents_WithMultipleEvents_PublishesAllAndClears()
  {
    var entity = new TestEntity();
    entity.AddEvent(new TestDomainEvent());
    entity.AddEvent(new TestDomainEvent());
    entity.AddEvent(new TestDomainEvent());

    await _dispatcher.DispatchAndClearEvents([entity]);

    await _mediator.Received(3).Publish(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>());
    entity.DomainEvents.ShouldBeEmpty();
  }

  [Fact]
  public async Task DispatchAndClearEvents_WithMultipleEntities_PublishesAllEvents()
  {
    var entity1 = new TestEntity();
    entity1.AddEvent(new TestDomainEvent());
    var entity2 = new TestEntity();
    entity2.AddEvent(new TestDomainEvent());
    entity2.AddEvent(new TestDomainEvent());

    await _dispatcher.DispatchAndClearEvents([entity1, entity2]);

    await _mediator.Received(3).Publish(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>());
    entity1.DomainEvents.ShouldBeEmpty();
    entity2.DomainEvents.ShouldBeEmpty();
  }

  [Fact]
  public async Task DispatchAndClearEvents_WithNoEvents_ClearsWithoutPublishing()
  {
    var entity = new TestEntity();

    await _dispatcher.DispatchAndClearEvents([entity]);

    await _mediator.DidNotReceive().Publish(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>());
    entity.DomainEvents.ShouldBeEmpty();
  }

  [Fact]
  public async Task DispatchAndClearEvents_WithEmptyCollection_DoesNothing()
  {
    await _dispatcher.DispatchAndClearEvents([]);

    await _mediator.DidNotReceive().Publish(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>());
  }

  private class TestEntity : HasDomainEventsBase
  {
    public void AddEvent(IDomainEvent domainEvent) => RegisterDomainEvent((DomainEventBase)domainEvent);
  }

  private class TestDomainEvent : DomainEventBase;
}
