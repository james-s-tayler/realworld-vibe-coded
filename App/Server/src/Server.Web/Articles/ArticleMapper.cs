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

    // Calculate user-specific values
    // Note: The article entity should already have FavoritedBy and Following loaded
    // via appropriate specifications when fetched
    var isFavorited = currentUserId.HasValue && article.FavoritedBy.Any(u => u.Id == currentUserId.Value);

    // For following status, we need to check if current user follows the article author
    // This requires the current user entity with Following relationship
    // However, to avoid extra DB call here, we'll need the handler to pass this info
    // For now, we'll get it from the repository (this is a temporary solution)
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
