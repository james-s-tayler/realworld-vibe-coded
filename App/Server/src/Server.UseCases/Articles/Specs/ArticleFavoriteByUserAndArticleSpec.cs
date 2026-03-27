using Ardalis.Specification;
using Server.Core.ArticleFavoriteAggregate;

namespace Server.UseCases.Articles.Specs;

public class ArticleFavoriteByUserAndArticleSpec : Specification<ArticleFavorite>
{
  public ArticleFavoriteByUserAndArticleSpec(Guid userId, Guid articleId)
  {
    Query.Where(f => f.UserId == userId && f.ArticleId == articleId);
  }
}
