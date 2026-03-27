using Ardalis.Specification;
using Server.Core.ArticleAggregate;

namespace Server.UseCases.Articles.Specs;

public class FeedArticlesSpec : Specification<Article>
{
  public FeedArticlesSpec(List<Guid> followedUserIds, int limit, int offset)
  {
    Query.Include(a => a.Author)
      .Where(a => followedUserIds.Contains(a.AuthorId))
      .OrderByDescending(a => a.CreatedAt)
      .Skip(offset)
      .Take(limit);
  }
}
