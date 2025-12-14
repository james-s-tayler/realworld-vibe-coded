namespace Server.Core.ArticleAggregate.Specifications.Articles;

public class ArticlesByAuthorSpec : Specification<Article>
{
  public ArticlesByAuthorSpec(string authorUsername)
  {
    Query.Where(x => x.Author.UserName == authorUsername)
         .Include(x => x.Author)
         .Include(x => x.Tags)
         .Include(x => x.FavoritedBy)
         .OrderByDescending(x => x.CreatedAt);
  }
}
