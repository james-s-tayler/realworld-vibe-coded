using Ardalis.Specification;
using Server.Core.ArticleAggregate;

namespace Server.UseCases.Articles.Specs;

public class CountArticlesSpec : Specification<Article>
{
  public CountArticlesSpec(string? author, string? tag, string? favorited)
  {
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

    Query.Include(a => a.Author);
  }
}
