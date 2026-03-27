using Server.Core.ArticleAggregate;
using Server.Core.ArticleFavoriteAggregate;
using Server.Core.UserFollowingAggregate;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Persistence;
using Server.UseCases.Articles.Specs;
using Server.UseCases.Profiles;

namespace Server.UseCases.Articles.Get;

public class GetArticleHandler(
  IReadRepository<Article> articleRepo,
  IReadRepository<ArticleFavorite> favoriteRepo,
  IReadRepository<UserFollowing> followingRepo)
  : IQueryHandler<GetArticleQuery, ArticleResult>
{
  public async Task<Result<ArticleResult>> Handle(GetArticleQuery request, CancellationToken cancellationToken)
  {
    var article = await articleRepo.FirstOrDefaultAsync(
      new ArticleBySlugSpec(request.Slug), cancellationToken);

    if (article == null)
    {
      return Result<ArticleResult>.NotFound(request.Slug);
    }

    var favorited = false;
    var authorFollowing = false;

    if (request.CurrentUserId.HasValue)
    {
      favorited = await favoriteRepo.AnyAsync(
        new ArticleFavoriteByUserAndArticleSpec(request.CurrentUserId.Value, article.Id),
        cancellationToken);

      authorFollowing = await followingRepo.AnyAsync(
        new UserFollowingByUsersSpec(request.CurrentUserId.Value, article.AuthorId),
        cancellationToken);
    }

    var favoritesCount = await favoriteRepo.CountAsync(
      new ArticleFavoritesByArticleSpec(article.Id), cancellationToken);

    return Result<ArticleResult>.Success(new ArticleResult(article, favorited, favoritesCount, authorFollowing));
  }
}
