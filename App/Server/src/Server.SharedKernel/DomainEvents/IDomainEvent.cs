using MediatR;

namespace Server.SharedKernel.DomainEvents;

public interface IDomainEvent : INotification
{
  DateTime DateOccurred { get; }
}
