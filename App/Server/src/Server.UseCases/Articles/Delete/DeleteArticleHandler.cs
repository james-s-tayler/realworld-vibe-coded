using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Specifications.Articles;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Persistence;

namespace Server.UseCases.Articles.Delete;

public class DeleteArticleHandler(IRepository<Article> _articleRepository)
  : ICommandHandler<DeleteArticleCommand, Article>
{
  public async Task<Result<Article>> Handle(DeleteArticleCommand request, CancellationToken cancellationToken)
  {
    var article = await _articleRepository.FirstOrDefaultAsync(
      new ArticleBySlugSpec(request.Slug), cancellationToken);

    if (article == null)
    {
      return Result<Article>.NotFound(request.Slug);
    }

    if (article.AuthorId != request.UserId)
    {
      return Result<Article>.Forbidden(new ErrorDetail("Forbidden", "You can only delete your own articles"));
    }

    await _articleRepository.DeleteAsync(article, cancellationToken);

    return Result<Article>.NoContent();
  }
}
