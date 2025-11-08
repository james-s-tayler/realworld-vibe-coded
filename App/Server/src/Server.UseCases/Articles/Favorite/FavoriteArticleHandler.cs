using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Specifications.Articles;
using Server.Core.UserAggregate;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Persistence;

namespace Server.UseCases.Articles.Favorite;

public class FavoriteArticleHandler(IRepository<Article> articleRepository, IRepository<User> userRepository)
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

    var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
    if (user == null)
    {
      return Result<Article>.ErrorMissingRequiredEntity(typeof(User), request.UserId);
    }

    article.AddToFavorites(user);
    await articleRepository.UpdateAsync(article, cancellationToken);

    return Result<Article>.Success(article);
  }
}
