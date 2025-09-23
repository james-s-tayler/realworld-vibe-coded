using Server.Core.ArticleAggregate;

namespace Server.Core.ArticleAggregate.Specifications;

public class ArticleSpec : Specification<Article>
{
  public ArticleSpec()
  {
    Query.Include(x => x.Author)
         .Include(x => x.Tags)
         .Include(x => x.FavoritedBy)
         .OrderByDescending(x => x.CreatedAt);
  }
}

public class ArticlesByTagSpec : Specification<Article>
{
  public ArticlesByTagSpec(string tagName)
  {
    Query.Where(x => x.Tags.Any(t => t.Name == tagName))
         .Include(x => x.Author)
         .Include(x => x.Tags)
         .Include(x => x.FavoritedBy)
         .OrderByDescending(x => x.CreatedAt);
  }
}

public class ArticlesByAuthorSpec : Specification<Article>
{
  public ArticlesByAuthorSpec(string authorUsername)
  {
    Query.Where(x => x.Author.Username == authorUsername)
         .Include(x => x.Author)
         .Include(x => x.Tags)
         .Include(x => x.FavoritedBy)
         .OrderByDescending(x => x.CreatedAt);
  }
}

public class ArticlesFavoritedByUserSpec : Specification<Article>
{
  public ArticlesFavoritedByUserSpec(string username)
  {
    Query.Where(x => x.FavoritedBy.Any(u => u.Username == username))
         .Include(x => x.Author)
         .Include(x => x.Tags)
         .Include(x => x.FavoritedBy)
         .OrderByDescending(x => x.CreatedAt);
  }
}
