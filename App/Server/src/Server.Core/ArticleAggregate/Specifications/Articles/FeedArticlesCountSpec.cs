namespace Server.Core.ArticleAggregate.Specifications.Articles;

public class FeedArticlesCountSpec : Specification<Article>
{
  public FeedArticlesCountSpec(List<Guid> followedUserIds)
  {
    Query.Include(x => x.Author)
         .Include(x => x.Tags)
         .Include(x => x.FavoritedBy)
         .Where(x => followedUserIds.Contains(x.AuthorId))
         .AsNoTracking();
  }
}
