using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Specifications.Articles;
using Server.Core.AuthorAggregate;
using Server.Core.AuthorAggregate.Specifications;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Persistence;

namespace Server.UseCases.Articles.Favorite;

public class FavoriteArticleHandler(IRepository<Article> articleRepository, IRepository<Author> authorRepository)
  : ICommandHandler<FavoriteArticleCommand, Article>
{
  public async Task<Result<Article>> Handle(FavoriteArticleCommand request, CancellationToken cancellationToken)
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

    article.AddToFavorites(author);
    await articleRepository.UpdateAsync(article, cancellationToken);

    return Result<Article>.Success(article);
  }
}
