using Server.Core.ArticleAggregate;
using Server.Core.ArticleFavoriteAggregate;
using Server.Core.UserFollowingAggregate;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Persistence;
using Server.UseCases.Articles.Specs;
using Server.UseCases.Profiles;

namespace Server.UseCases.Articles.Feed;

public class FeedArticlesHandler(
  IReadRepository<Article> articleRepo,
  IReadRepository<ArticleFavorite> favoriteRepo,
  IReadRepository<UserFollowing> followingRepo)
  : IQueryHandler<FeedArticlesQuery, ArticlesListResult>
{
  public async Task<Result<ArticlesListResult>> Handle(FeedArticlesQuery request, CancellationToken cancellationToken)
  {
    var followedUserIds = await followingRepo.ListAsync(
      new UserFollowingByFollowerSpec(request.CurrentUserId), cancellationToken);

    var followedIds = followedUserIds.Select(f => f.FollowedId).ToList();

    var articles = await articleRepo.ListAsync(
      new FeedArticlesSpec(followedIds, request.Limit, request.Offset), cancellationToken);

    var totalCount = await articleRepo.CountAsync(
      new CountFeedArticlesSpec(followedIds), cancellationToken);

    var results = new List<ArticleResult>();
    foreach (var article in articles)
    {
      var favorited = await favoriteRepo.AnyAsync(
        new ArticleFavoriteByUserAndArticleSpec(request.CurrentUserId, article.Id),
        cancellationToken);

      var favoritesCount = await favoriteRepo.CountAsync(
        new ArticleFavoritesByArticleSpec(article.Id), cancellationToken);

      results.Add(new ArticleResult(article, favorited, favoritesCount, true));
    }

    return Result<ArticlesListResult>.Success(new ArticlesListResult(results, totalCount));
  }
}
