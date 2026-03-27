using Server.Core.ArticleAggregate;
using Server.Core.ArticleFavoriteAggregate;
using Server.Core.UserFollowingAggregate;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Persistence;
using Server.UseCases.Articles.Specs;
using Server.UseCases.Profiles;

namespace Server.UseCases.Articles.Update;

public class UpdateArticleHandler(
  IRepository<Article> articleRepo,
  IReadRepository<ArticleFavorite> favoriteRepo,
  IReadRepository<UserFollowing> followingRepo)
  : ICommandHandler<UpdateArticleCommand, ArticleResult>
{
  public async Task<Result<ArticleResult>> Handle(UpdateArticleCommand request, CancellationToken cancellationToken)
  {
    var article = await articleRepo.FirstOrDefaultAsync(
      new ArticleBySlugSpec(request.Slug), cancellationToken);

    if (article == null)
    {
      return Result<ArticleResult>.NotFound(request.Slug);
    }

    if (article.AuthorId != request.CurrentUserId)
    {
      return Result<ArticleResult>.Forbidden();
    }

    if (!string.IsNullOrEmpty(request.Title))
    {
      var newSlug = SlugHelper.GenerateSlug(request.Title);
      if (newSlug != article.Slug)
      {
        var slugExists = await articleRepo.AnyAsync(new ArticleBySlugSpec(newSlug), cancellationToken);
        if (slugExists)
        {
          return Result<ArticleResult>.Invalid(new ErrorDetail("slug", "has already been taken."));
        }

        article.Slug = newSlug;
      }

      article.Title = request.Title;
    }

    if (!string.IsNullOrEmpty(request.Description))
    {
      article.Description = request.Description;
    }

    if (!string.IsNullOrEmpty(request.Body))
    {
      article.Body = request.Body;
    }

    await articleRepo.UpdateAsync(article, cancellationToken);

    var favorited = await favoriteRepo.AnyAsync(
      new ArticleFavoriteByUserAndArticleSpec(request.CurrentUserId, article.Id),
      cancellationToken);

    var favoritesCount = await favoriteRepo.CountAsync(
      new ArticleFavoritesByArticleSpec(article.Id), cancellationToken);

    var authorFollowing = await followingRepo.AnyAsync(
      new UserFollowingByUsersSpec(request.CurrentUserId, article.AuthorId),
      cancellationToken);

    return Result<ArticleResult>.Success(new ArticleResult(article, favorited, favoritesCount, authorFollowing));
  }
}
