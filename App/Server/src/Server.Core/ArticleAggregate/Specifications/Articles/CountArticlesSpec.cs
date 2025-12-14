namespace Server.Core.ArticleAggregate.Specifications.Articles;

public class CountArticlesSpec : Specification<Article>
{
  public CountArticlesSpec(
    string? tag = null,
    string? author = null,
    string? favorited = null)
  {
    Query.AsNoTracking();

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
  }
}
