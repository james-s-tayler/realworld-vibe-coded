namespace Server.Core.ArticleAggregate.Specifications.Articles;

public class ListArticlesSpec : Specification<Article>
{
  public ListArticlesSpec(
    string? tag = null,
    string? author = null,
    string? favorited = null,
    int limit = 20,
    int offset = 0)
  {
    Query.Include(x => x.Author)
         .Include(x => x.Tags)
         .Include(x => x.FavoritedBy)
         .OrderByDescending(x => x.CreatedAt)
         .AsNoTracking();

    if (!string.IsNullOrEmpty(tag))
    {
      Query.Where(x => x.Tags.Any(t => t.Name == tag));
    }

    if (!string.IsNullOrEmpty(author))
    {
      Query.Where(x => x.Author.UserName == author);
    }

    if (!string.IsNullOrEmpty(favorited))
    {
      Query.Where(x => x.FavoritedBy.Any(u => u.UserName == favorited));
    }

    Query.Skip(offset)
         .Take(Math.Min(limit, 100));
  }
}
