using Ardalis.Specification;
using Server.Core.CommentAggregate;

namespace Server.UseCases.Comments.Specs;

public class CommentsByArticleSpec : Specification<Comment>
{
  public CommentsByArticleSpec(Guid articleId)
  {
    Query.Where(c => c.ArticleId == articleId)
      .Include(c => c.Author)
      .OrderBy(c => c.CreatedAt);
  }
}
