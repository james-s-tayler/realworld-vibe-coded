using Server.SharedKernel.Persistence;

namespace Server.Core.ArticleAggregate;

public class Tag : EntityBase, IAggregateRoot
{
  public Tag(string name)
  {
    Name = Guard.Against.NullOrEmpty(name);
  }

  private Tag()
  {
  } // For EF Core

  public string Name { get; private set; } = string.Empty;

  public List<Article> Articles { get; private set; } = new();
}
