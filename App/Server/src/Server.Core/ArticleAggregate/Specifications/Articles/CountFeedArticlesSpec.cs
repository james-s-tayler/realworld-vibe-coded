namespace Server.Core.ArticleAggregate.Specifications.Articles;

public class CountFeedArticlesSpec : Specification<Article>
{
  public CountFeedArticlesSpec(List<Guid> followedUserIds)
  {
    Query.Where(x => followedUserIds.Contains(x.AuthorId))
         .AsNoTracking();
  }
}
