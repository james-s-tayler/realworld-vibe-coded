using MediatR;

namespace Server.SharedKernel;

public interface IDomainEvent : INotification
{
  DateTime DateOccurred { get; }
}
