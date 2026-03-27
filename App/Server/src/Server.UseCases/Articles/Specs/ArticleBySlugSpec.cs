using Ardalis.Specification;
using Server.Core.ArticleAggregate;

namespace Server.UseCases.Articles.Specs;

public class ArticleBySlugSpec : Specification<Article>
{
  public ArticleBySlugSpec(string slug)
  {
    Query.Where(a => a.Slug == slug)
      .Include(a => a.Author);
  }
}
