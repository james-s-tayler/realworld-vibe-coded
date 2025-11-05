namespace Server.Core.ArticleAggregate.Specifications.Articles;

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
