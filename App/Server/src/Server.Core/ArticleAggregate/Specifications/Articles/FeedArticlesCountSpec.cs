namespace Server.Core.ArticleAggregate.Specifications.Articles;

public class FeedArticlesCountSpec : Specification<Article>
{
  public FeedArticlesCountSpec(List<Guid> followedUserIds)
  {
    Query.Where(x => followedUserIds.Contains(x.AuthorId))
         .AsNoTracking();
  }
}
