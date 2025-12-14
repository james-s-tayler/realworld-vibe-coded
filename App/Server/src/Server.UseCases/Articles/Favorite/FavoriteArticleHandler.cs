using Microsoft.AspNetCore.Identity;
using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Specifications.Articles;
using Server.Core.IdentityAggregate;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Persistence;

namespace Server.UseCases.Articles.Favorite;

public class FavoriteArticleHandler(IRepository<Article> articleRepository, UserManager<ApplicationUser> userManager)
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

    var user = await userManager.FindByIdAsync(request.UserId.ToString());
    if (user == null)
    {
      return Result<Article>.ErrorMissingRequiredEntity(typeof(ApplicationUser), request.UserId);
    }

    article.AddToFavorites(user);
    await articleRepository.UpdateAsync(article, cancellationToken);

    return Result<Article>.Success(article);
  }
}
