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
    var dto = await MapArticleToDtoAsync(article, Resolve<IUserContext>(), Resolve<AppDbContext>(), ct);
    return new ArticleResponse { Article = dto };
  }

  internal static async Task<ArticleDto> MapArticleToDtoAsync(
    Article article,
    IUserContext userContext,
    AppDbContext dbContext,
    CancellationToken ct)
  {
    var currentUserId = userContext.GetCurrentUserId();
    var isFavorited = article.IsFavoritedBy(currentUserId);

    var isFollowing = false;
    if (currentUserId.HasValue)
    {
      isFollowing = await dbContext.Set<AuthorFollowing>()
        .AnyAsync(af => af.FollowerId == currentUserId.Value && af.FollowedId == article.AuthorId, ct);
    }

    return new ArticleDto(
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
  }
}
