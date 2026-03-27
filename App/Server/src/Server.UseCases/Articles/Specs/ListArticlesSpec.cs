using Ardalis.Specification;
using Server.Core.ArticleAggregate;

namespace Server.UseCases.Articles.Specs;

public class ListArticlesSpec : Specification<Article>
{
  public ListArticlesSpec(string? author, string? tag, string? favorited, int limit, int offset)
  {
    Query.Include(a => a.Author)
      .OrderByDescending(a => a.CreatedAt)
      .Skip(offset)
      .Take(limit);

    if (!string.IsNullOrEmpty(author))
    {
      Query.Where(a => a.Author.UserName == author);
    }

    if (!string.IsNullOrEmpty(tag))
    {
      Query.Where(a => a.TagList.Contains(tag));
    }

    if (!string.IsNullOrEmpty(favorited))
    {
      Query.Where(a => a.Favorites.Any(f => f.User.UserName == favorited));
    }
  }
}
