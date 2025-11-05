namespace Server.Core.ArticleAggregate.Specifications.Articles;

public class ArticleBySlugSpec : Specification<Article>
{
  public ArticleBySlugSpec(string slug)
  {
    Query.Where(x => x.Slug == slug)
         .Include(x => x.Author)
         .Include(x => x.Tags)
         .Include(x => x.FavoritedBy)
         .Include(x => x.Comments)
         .ThenInclude(c => c.Author);
  }
}
