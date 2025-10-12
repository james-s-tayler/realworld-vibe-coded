using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Specifications;
using Server.Core.UserAggregate;

namespace Server.UseCases.Articles.Favorite;

public class FavoriteArticleHandler(IRepository<Article> _articleRepository, IRepository<User> _userRepository)
  : ICommandHandler<FavoriteArticleCommand, Result<Article>>
{
  public async Task<Result<Article>> Handle(FavoriteArticleCommand request, CancellationToken cancellationToken)
  {
    var article = await _articleRepository.FirstOrDefaultAsync(
      new ArticleBySlugSpec(request.Slug), cancellationToken);

    if (article == null)
    {
      return Result.NotFound("Article not found");
    }

    var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
    if (user == null)
    {
      return Result.Error("User not found");
    }

    article.AddToFavorites(user);
    await _articleRepository.UpdateAsync(article, cancellationToken);
    await _articleRepository.SaveChangesAsync(cancellationToken);

    return Result.Success(article);
  }
}
