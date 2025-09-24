using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Specifications;
using Server.Core.Interfaces;

namespace Server.UseCases.Articles.Delete;

public class DeleteArticleHandler(IRepository<Article> _articleRepository)
  : ICommandHandler<DeleteArticleCommand, Result>
{
  public async Task<Result> Handle(DeleteArticleCommand request, CancellationToken cancellationToken)
  {
    var article = await _articleRepository.FirstOrDefaultAsync(
      new ArticleBySlugSpec(request.Slug), cancellationToken);

    if (article == null)
    {
      return Result.NotFound("Article not found");
    }

    if (article.AuthorId != request.UserId)
    {
      return Result.Forbidden("You can only delete your own articles");
    }

    await _articleRepository.DeleteAsync(article, cancellationToken);
    await _articleRepository.SaveChangesAsync(cancellationToken);

    return Result.Success();
  }
}