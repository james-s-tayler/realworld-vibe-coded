using Server.SharedKernel.DomainEvents;

namespace Server.SharedKernel.Persistence;

/// <summary>
/// A base type for domain events. Depends on MediatR INotification.
/// Includes DateOccurred which is set on creation.
/// </summary>
public abstract class DomainEventBase : IDomainEvent
{
  public DateTime DateOccurred { get; protected set; } = DateTime.UtcNow;
}
