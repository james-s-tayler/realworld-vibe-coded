using Server.Core.ArticleAggregate;
using Server.Core.IdentityAggregate;
using Server.SharedKernel.Persistence;

namespace Server.Core.CommentAggregate;

public class Comment : EntityBase, IAggregateRoot
{
  public const int BodyMaxLength = 50000;

  public string Body { get; set; } = default!;

  public Guid ArticleId { get; set; }

  public Article Article { get; set; } = default!;

  public Guid AuthorId { get; set; }

  public ApplicationUser Author { get; set; } = default!;
}
