using Ardalis.SharedKernel;
using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Dtos;
using Server.Core.Interfaces;
using Server.Core.UserAggregate;
using Server.UseCases.Articles;

namespace Server.Web.Articles;

/// <summary>
/// FastEndpoints mapper for Article entity to ArticleResponse DTO
/// Maps domain entity to response DTO with current user context for favorited/following status
/// </summary>
public class ArticleMapper : ResponseMapper<ArticleResponse, Article>
{
  public override ArticleResponse FromEntity(Article article)
  {
    // Resolve current user service to get authentication context
    var currentUserService = Resolve<ICurrentUserService>();
    var currentUserId = currentUserService.GetCurrentUserId();

    // Use domain methods to compute user-specific values
    var isFavorited = article.IsFavoritedBy(currentUserId);

    // For following status, get current user and use domain method
    bool isFollowing = false;
    if (currentUserId.HasValue)
    {
      var userRepository = Resolve<IRepository<User>>();
      var userSpec = new Server.Core.UserAggregate.Specifications.UserWithFollowingSpec(currentUserId.Value);
      var currentUser = userRepository.FirstOrDefaultAsync(userSpec).GetAwaiter().GetResult();
      isFollowing = currentUser?.IsFollowing(article.AuthorId) ?? false;
    }

    var articleDto = new ArticleDto(
      article.Slug,
      article.Title,
      article.Description,
      article.Body,
      article.Tags.Select(t => t.Name).ToList(),
      article.CreatedAt,
      article.UpdatedAt,
      isFavorited,
      article.FavoritesCount,
      new AuthorDto(
        article.Author.Username,
        article.Author.Bio ?? string.Empty,
        article.Author.Image,
        isFollowing
      )
    );

    return new ArticleResponse { Article = articleDto };
  }
}
