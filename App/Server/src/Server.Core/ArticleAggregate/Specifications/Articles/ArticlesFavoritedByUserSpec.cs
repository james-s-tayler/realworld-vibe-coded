namespace Server.Core.ArticleAggregate.Specifications.Articles;

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
