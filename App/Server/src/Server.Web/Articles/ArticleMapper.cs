using Microsoft.EntityFrameworkCore;
using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Dtos;
using Server.Core.AuthorAggregate;
using Server.Infrastructure.Data;
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

    // Check if current user follows the article author by querying AuthorFollowing directly
    // This avoids loading ApplicationUser with all ASP.NET Identity fields
    var isFollowing = false;
    if (currentUserId.HasValue)
    {
      var dbContext = Resolve<AppDbContext>();
      isFollowing = await dbContext.Set<AuthorFollowing>()
        .AnyAsync(af => af.FollowerId == currentUserId.Value && af.FollowedId == article.AuthorId, ct);
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
        article.Author.Bio,
        article.Author.Image,
        isFollowing
      )
    );

    return new ArticleResponse { Article = articleDto };
  }
}
