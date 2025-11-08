using System.ComponentModel.DataAnnotations.Schema;
using Server.SharedKernel.Persistence;

namespace Server.SharedKernel.DomainEvents;

public abstract class HasDomainEventsBase : IHasDomainEvents
{
  private readonly List<IDomainEvent> _domainEvents = new();

  [NotMapped]
  public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

  public void ClearDomainEvents() => _domainEvents.Clear();

  protected void RegisterDomainEvent(DomainEventBase domainEvent) => _domainEvents.Add(domainEvent);
}
