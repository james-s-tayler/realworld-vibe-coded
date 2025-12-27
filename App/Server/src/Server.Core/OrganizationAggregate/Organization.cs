using Server.SharedKernel.Persistence;

namespace Server.Core.OrganizationAggregate;

public class Organization : EntityBase, IAggregateRoot
{
  public const int NameMaxLength = 200;
  public const int IdentifierMaxLength = 50;

  public Organization(string name, string identifier)
  {
    Name = Guard.Against.NullOrEmpty(name);
    Identifier = Guard.Against.NullOrEmpty(identifier);
  }

  // For EF Core
  private Organization()
  {
  }

  public string Name { get; private set; } = string.Empty;

  public string Identifier { get; private set; } = string.Empty;
}
