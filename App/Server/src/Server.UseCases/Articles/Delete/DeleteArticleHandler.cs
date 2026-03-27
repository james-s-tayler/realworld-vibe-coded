using Server.Core.ArticleAggregate;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Persistence;
using Server.UseCases.Articles.Specs;

namespace Server.UseCases.Articles.Delete;

public class DeleteArticleHandler(IRepository<Article> articleRepo)
  : ICommandHandler<DeleteArticleCommand, bool>
{
  public async Task<Result<bool>> Handle(DeleteArticleCommand request, CancellationToken cancellationToken)
  {
    var article = await articleRepo.FirstOrDefaultAsync(
      new ArticleBySlugSpec(request.Slug), cancellationToken);

    if (article == null)
    {
      return Result<bool>.NotFound(request.Slug);
    }

    if (article.AuthorId != request.CurrentUserId)
    {
      return Result<bool>.Forbidden();
    }

    await articleRepo.DeleteAsync(article, cancellationToken);

    return Result<bool>.NoContent();
  }
}
