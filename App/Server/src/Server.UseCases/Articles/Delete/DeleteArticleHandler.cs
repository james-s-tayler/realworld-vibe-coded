using Microsoft.Extensions.Localization;
using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Specifications.Articles;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Persistence;
using Server.SharedKernel.Resources;

namespace Server.UseCases.Articles.Delete;

public class DeleteArticleHandler(IRepository<Article> articleRepository, IStringLocalizer localizer)
  : ICommandHandler<DeleteArticleCommand, Article>
{
  public async Task<Result<Article>> Handle(DeleteArticleCommand request, CancellationToken cancellationToken)
  {
    var article = await articleRepository.FirstOrDefaultAsync(
      new ArticleBySlugSpec(request.Slug), cancellationToken);

    if (article == null)
    {
      return Result<Article>.NotFound(request.Slug);
    }

    if (article.AuthorId != request.UserId)
    {
      return Result<Article>.Forbidden(new ErrorDetail("Forbidden", localizer[SharedResource.Keys.CanOnlyDeleteOwnArticles]));
    }

    await articleRepository.DeleteAsync(article, cancellationToken);

    return Result<Article>.NoContent();
  }
}
