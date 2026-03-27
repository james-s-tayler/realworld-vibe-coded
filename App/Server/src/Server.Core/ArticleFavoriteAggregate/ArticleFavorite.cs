using Server.Core.ArticleAggregate;
using Server.Core.IdentityAggregate;
using Server.SharedKernel.Persistence;

namespace Server.Core.ArticleFavoriteAggregate;

public class ArticleFavorite : EntityBase, IAggregateRoot
{
  public Guid UserId { get; set; }

  public ApplicationUser User { get; set; } = default!;

  public Guid ArticleId { get; set; }

  public Article Article { get; set; } = default!;
}
