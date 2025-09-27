using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Dtos;
using Server.Core.UserAggregate;

namespace Server.UseCases.Articles;

/// <summary>
/// Static mappers for Article-related entities to DTOs to reduce duplication across handlers
/// </summary>
public static class ArticleMappers
{
  /// <summary>
  /// Maps Article entity to ArticleDto with current user context for favorited and following status
  /// </summary>
  public static ArticleDto MapToDto(Article article, User? currentUser = null)
  {
    var isFavorited = currentUser != null && article.FavoritedBy.Any(u => u.Id == currentUser.Id);
    var isFollowing = currentUser?.IsFollowing(article.AuthorId) ?? false;

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
        article.Author.Bio ?? string.Empty,
        article.Author.Image,
        isFollowing
      )
    );
  }

  /// <summary>
  /// Maps Article entity to ArticleDto with explicit favorited status override
  /// </summary>
  public static ArticleDto MapToDto(Article article, User? currentUser, bool isFavorited)
  {
    var isFollowing = currentUser?.IsFollowing(article.AuthorId) ?? false;

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
        article.Author.Bio ?? string.Empty,
        article.Author.Image,
        isFollowing
      )
    );
  }

  /// <summary>
  /// Generates a URL-friendly slug from an article title
  /// </summary>
  public static string GenerateSlug(string title)
  {
    return title.ToLowerInvariant()
      .Replace(" ", "-")
      .Replace(".", "")
      .Replace(",", "")
      .Replace("!", "")
      .Replace("?", "")
      .Replace("'", "")
      .Replace("\"", "");
  }
}
