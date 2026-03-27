using Server.Core.ArticleAggregate;
using Server.Core.ArticleFavoriteAggregate;
using Server.Core.UserFollowingAggregate;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Persistence;
using Server.UseCases.Articles.Specs;
using Server.UseCases.Profiles;

namespace Server.UseCases.Articles.List;

public class ListArticlesHandler(
  IReadRepository<Article> articleRepo,
  IReadRepository<ArticleFavorite> favoriteRepo,
  IReadRepository<UserFollowing> followingRepo)
  : IQueryHandler<ListArticlesQuery, ArticlesListResult>
{
  public async Task<Result<ArticlesListResult>> Handle(ListArticlesQuery request, CancellationToken cancellationToken)
  {
    var spec = new ListArticlesSpec(request.Author, request.Tag, request.Favorited, request.Limit, request.Offset);
    var articles = await articleRepo.ListAsync(spec, cancellationToken);

    var countSpec = new CountArticlesSpec(request.Author, request.Tag, request.Favorited);
    var totalCount = await articleRepo.CountAsync(countSpec, cancellationToken);

    var results = new List<ArticleResult>();
    foreach (var article in articles)
    {
      var favorited = await favoriteRepo.AnyAsync(
        new ArticleFavoriteByUserAndArticleSpec(request.CurrentUserId, article.Id),
        cancellationToken);

      var favoritesCount = await favoriteRepo.CountAsync(
        new ArticleFavoritesByArticleSpec(article.Id), cancellationToken);

      var authorFollowing = await followingRepo.AnyAsync(
        new UserFollowingByUsersSpec(request.CurrentUserId, article.AuthorId),
        cancellationToken);

      results.Add(new ArticleResult(article, favorited, favoritesCount, authorFollowing));
    }

    return Result<ArticlesListResult>.Success(new ArticlesListResult(results, totalCount));
  }
}
