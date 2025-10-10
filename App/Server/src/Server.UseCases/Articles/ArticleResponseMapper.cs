using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Dtos;
using Server.Core.UserAggregate;

namespace Server.UseCases.Articles;

/// <summary>
/// FastEndpoints-style mapper for Article entity to ArticleResponse
/// Follows the FastEndpoints Domain Entity Mapping pattern
/// </summary>
public class ArticleResponseMapper
{
  private readonly User? _currentUser;

  public ArticleResponseMapper(User? currentUser = null)
  {
    _currentUser = currentUser;
  }

  /// <summary>
  /// Maps Article entity to ArticleResponse DTO
  /// </summary>
  public ArticleResponse FromEntity(Article article)
  {
    var articleDto = MapToDto(article);
    return new ArticleResponse { Article = articleDto };
  }

  /// <summary>
  /// Maps Article entity to ArticleDto with explicit favorited override
  /// </summary>
  public ArticleResponse FromEntity(Article article, bool isFavorited)
  {
    var articleDto = MapToDto(article, isFavorited);
    return new ArticleResponse { Article = articleDto };
  }

  /// <summary>
  /// Maps collection of Article entities to ArticlesResponse
  /// </summary>
  public ArticlesResponse FromEntities(IEnumerable<Article> articles)
  {
    var articleDtos = articles.Select(MapToDto).ToList();
    return new ArticlesResponse(articleDtos, articleDtos.Count);
  }

  private ArticleDto MapToDto(Article article)
  {
    var isFavorited = _currentUser != null && article.FavoritedBy.Any(u => u.Id == _currentUser.Id);
    var isFollowing = _currentUser?.IsFollowing(article.AuthorId) ?? false;

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

  private ArticleDto MapToDto(Article article, bool isFavorited)
  {
    var isFollowing = _currentUser?.IsFollowing(article.AuthorId) ?? false;

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
}
