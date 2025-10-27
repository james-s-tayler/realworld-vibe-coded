using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Specifications;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Persistence;

namespace Server.UseCases.Articles.Delete;

public class DeleteArticleHandler(IRepository<Article> _articleRepository)
  : ICommandHandler<DeleteArticleCommand, Unit>
{
  public async Task<Result<Unit>> Handle(DeleteArticleCommand request, CancellationToken cancellationToken)
  {
    var article = await _articleRepository.FirstOrDefaultAsync(
      new ArticleBySlugSpec(request.Slug), cancellationToken);

    if (article == null)
    {
      return Result<Unit>.NotFound("Article not found");
    }

    if (article.AuthorId != request.UserId)
    {
      return Result<Unit>.Forbidden("You can only delete your own articles");
    }

    await _articleRepository.DeleteAsync(article, cancellationToken);
    await _articleRepository.SaveChangesAsync(cancellationToken);

    return Result<Unit>.NoContent();
  }
}
