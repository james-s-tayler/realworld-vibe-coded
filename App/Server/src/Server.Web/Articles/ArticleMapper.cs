using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Dtos;
using Server.Core.IdentityAggregate;
using Server.UseCases.Articles;
using Server.UseCases.Interfaces;

namespace Server.Web.Articles;

/// <summary>
/// FastEndpoints mapper for Article entity to ArticleResponse DTO
/// Maps domain entity to response DTO with current user context for favorited/following status
/// </summary>
public class ArticleMapper : ResponseMapper<ArticleResponse, Article>
{
  public override async Task<ArticleResponse> FromEntityAsync(Article article, CancellationToken ct)
  {
    // Resolve current user service to get authentication context
    var currentUserService = Resolve<IUserContext>();
    var currentUserId = currentUserService.GetCurrentUserId();

    // Use domain methods to compute user-specific values
    var isFavorited = article.IsFavoritedBy(currentUserId);

    // Get current user for following status check
    ApplicationUser? currentUser = null;
    if (currentUserId.HasValue)
    {
      var userManager = Resolve<UserManager<ApplicationUser>>();
      currentUser = await userManager.Users
        .Include(u => u.Following)
        .FirstOrDefaultAsync(u => u.Id == currentUserId.Value, ct);
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
        article.Author.Bio,
        article.Author.Image,
        isFollowing
      )
    );

    return new ArticleResponse { Article = articleDto };
  }
}
