using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Specifications.Articles;
using Server.Core.AuthorAggregate;
using Server.Core.AuthorAggregate.Specifications;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Persistence;

namespace Server.UseCases.Articles.Unfavorite;

public class UnfavoriteArticleHandler(IRepository<Article> articleRepository, IRepository<Author> authorRepository)
  : ICommandHandler<UnfavoriteArticleCommand, Article>
{
  public async Task<Result<Article>> Handle(UnfavoriteArticleCommand request, CancellationToken cancellationToken)
  {
    var article = await articleRepository.FirstOrDefaultAsync(
      new ArticleBySlugSpec(request.Slug), cancellationToken);

    if (article == null)
    {
      return Result<Article>.NotFound(request.Slug);
    }

    var author = await authorRepository.FirstOrDefaultAsync(
      new AuthorByUserIdSpec(request.UserId), cancellationToken);
    if (author == null)
    {
      return Result<Article>.ErrorMissingRequiredEntity(typeof(Author), request.UserId);
    }

    article.RemoveFromFavorites(author);
    await articleRepository.UpdateAsync(article, cancellationToken);

    return Result<Article>.Success(article);
  }
}
