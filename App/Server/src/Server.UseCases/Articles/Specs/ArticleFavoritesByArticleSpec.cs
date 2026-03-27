using Ardalis.Specification;
using Server.Core.ArticleFavoriteAggregate;

namespace Server.UseCases.Articles.Specs;

public class ArticleFavoritesByArticleSpec : Specification<ArticleFavorite>
{
  public ArticleFavoritesByArticleSpec(Guid articleId)
  {
    Query.Where(f => f.ArticleId == articleId);
  }
}
