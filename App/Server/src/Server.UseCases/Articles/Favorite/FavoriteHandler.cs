using Server.Core.ArticleAggregate;
using Server.Core.ArticleFavoriteAggregate;
using Server.Core.UserFollowingAggregate;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Persistence;
using Server.UseCases.Articles.Specs;
using Server.UseCases.Profiles;

namespace Server.UseCases.Articles.Favorite;

public class FavoriteHandler(
  IReadRepository<Article> articleRepo,
  IRepository<ArticleFavorite> favoriteRepo,
  IReadRepository<UserFollowing> followingRepo)
  : ICommandHandler<FavoriteCommand, ArticleResult>
{
  public async Task<Result<ArticleResult>> Handle(FavoriteCommand request, CancellationToken cancellationToken)
  {
    var article = await articleRepo.FirstOrDefaultAsync(
      new ArticleBySlugSpec(request.Slug), cancellationToken);

    if (article == null)
    {
      return Result<ArticleResult>.NotFound(request.Slug);
    }

    var existing = await favoriteRepo.FirstOrDefaultAsync(
      new ArticleFavoriteByUserAndArticleSpec(request.UserId, article.Id), cancellationToken);

    if (existing == null)
    {
      var favorite = new ArticleFavorite
      {
        UserId = request.UserId,
        ArticleId = article.Id,
      };
      await favoriteRepo.AddAsync(favorite, cancellationToken);
    }

    var favoritesCount = await favoriteRepo.CountAsync(
      new ArticleFavoritesByArticleSpec(article.Id), cancellationToken);

    var authorFollowing = await followingRepo.AnyAsync(
      new UserFollowingByUsersSpec(request.UserId, article.AuthorId), cancellationToken);

    return Result<ArticleResult>.Success(new ArticleResult(article, true, favoritesCount, authorFollowing));
  }
}
