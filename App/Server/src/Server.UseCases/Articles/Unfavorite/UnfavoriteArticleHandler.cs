using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Specifications;
using Server.Core.UserAggregate;
using Server.Core.UserAggregate.Specifications;

namespace Server.UseCases.Articles.Unfavorite;

public class UnfavoriteArticleHandler(IRepository<Article> _articleRepository, IRepository<User> _userRepository)
  : ICommandHandler<UnfavoriteArticleCommand, Result<ArticleResponse>>
{
  public async Task<Result<ArticleResponse>> Handle(UnfavoriteArticleCommand request, CancellationToken cancellationToken)
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

    article.RemoveFromFavorites(user);
    await _articleRepository.UpdateAsync(article, cancellationToken);
    await _articleRepository.SaveChangesAsync(cancellationToken);

    // Load current user with following relationships to check following status
    var currentUserWithFollowing = await _userRepository.FirstOrDefaultAsync(
      new UserWithFollowingSpec(request.CurrentUserId), cancellationToken);

    // Use FastEndpoints-style mapper with explicit favorited = false since user just unfavorited
    var mapper = new ArticleResponseMapper(currentUserWithFollowing);
    var response = mapper.FromEntity(article, false);

    return Result.Success(response);
  }
}
