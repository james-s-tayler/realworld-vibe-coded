namespace Server.Core.ArticleAggregate.Specifications.Articles;

public class FeedArticlesSpec : Specification<Article>
{
  public FeedArticlesSpec(
    List<Guid> followedUserIds,
    int limit = 20,
    int offset = 0)
  {
    Query.Include(x => x.Author)
         .Include(x => x.Tags)
         .Include(x => x.FavoritedBy)
         .Where(x => followedUserIds.Contains(x.AuthorId))
         .OrderByDescending(x => x.CreatedAt)
         .Skip(offset)
         .Take(Math.Min(limit, 100))
         .AsNoTracking();
  }
}
