using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Dtos;
using Server.Core.Interfaces;
using Server.Core.UserAggregate;
using Server.SharedKernel;
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

    // Get current user for following status check
    User? currentUser = null;
    if (currentUserId.HasValue)
    {
      var userRepository = Resolve<IRepository<User>>();
      var userSpec = new Server.Core.UserAggregate.Specifications.UserWithFollowingSpec(currentUserId.Value);
      currentUser = userRepository.FirstOrDefaultAsync(userSpec).GetAwaiter().GetResult();
    }

    // Use domain method to check if current user follows the article author
    var isFollowing = article.IsAuthorFollowedBy(currentUser);

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
