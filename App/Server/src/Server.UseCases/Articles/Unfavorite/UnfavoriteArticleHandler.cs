using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Dtos;
using Server.Core.ArticleAggregate.Specifications;
using Server.Core.Interfaces;
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

    var isFollowing = currentUserWithFollowing?.IsFollowing(article.AuthorId) ?? false;

    var articleDto = new ArticleDto(
      article.Slug,
      article.Title,
      article.Description,
      article.Body,
      article.Tags.Select(t => t.Name).ToList(),
      article.CreatedAt,
      article.UpdatedAt,
      false, // User just unfavorited this article
      article.FavoritesCount,
      new AuthorDto(
        article.Author.Username,
        article.Author.Bio ?? string.Empty,
        article.Author.Image,
        isFollowing
      )
    );

    return Result.Success(new ArticleResponse { Article = articleDto });
  }
}
