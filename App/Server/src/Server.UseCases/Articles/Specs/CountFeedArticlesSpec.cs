using Ardalis.Specification;
using Server.Core.ArticleAggregate;

namespace Server.UseCases.Articles.Specs;

public class CountFeedArticlesSpec : Specification<Article>
{
  public CountFeedArticlesSpec(List<Guid> followedUserIds)
  {
    Query.Where(a => followedUserIds.Contains(a.AuthorId));
  }
}
