using Server.Core.ArticleAggregate;
using Server.SharedKernel.Persistence;

namespace Server.Core.TagAggregate;

public class Tag : EntityBase, IAggregateRoot
{
  public const int NameMaxLength = 100;

  public string Name { get; set; } = default!;

  public ICollection<Article> Articles { get; set; } = new List<Article>();
}
