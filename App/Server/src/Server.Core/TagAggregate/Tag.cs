using Ardalis.GuardClauses;
using Ardalis.SharedKernel;

namespace Server.Core.TagAggregate;

public class Tag : EntityBase, IAggregateRoot
{
  public Tag(string name)
  {
    Name = Guard.Against.NullOrEmpty(name, nameof(name));
  }

  public string Name { get; private set; } = default!;

  public Tag UpdateName(string newName)
  {
    Name = Guard.Against.NullOrEmpty(newName, nameof(newName));
    return this;
  }
}
