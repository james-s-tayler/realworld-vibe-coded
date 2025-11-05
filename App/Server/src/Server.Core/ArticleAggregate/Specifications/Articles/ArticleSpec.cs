namespace Server.Core.ArticleAggregate.Specifications.Articles;

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
