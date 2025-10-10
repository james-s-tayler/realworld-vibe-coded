using Ardalis.SharedKernel;
using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Dtos;
using Server.Core.Interfaces;
using Server.Core.UserAggregate;
using Server.Core.UserAggregate.Specifications;
using Server.UseCases.Articles;

namespace Server.Web.Articles;

/// <summary>
/// FastEndpoints mapper for Article entity to ArticleResponse
/// Used by endpoints via Map.FromEntityAsync or Resolve
/// </summary>
public class ArticleMapper : Mapper<EmptyRequest, ArticleResponse, Article>
{
  private readonly ICurrentUserService _currentUserService;
  private readonly IRepository<User> _userRepository;

  public ArticleMapper(ICurrentUserService currentUserService, IRepository<User> userRepository)
  {
    _currentUserService = currentUserService;
    _userRepository = userRepository;
  }

  public override Task<Article> ToEntityAsync(EmptyRequest r, CancellationToken ct = default)
  {
    throw new NotImplementedException("ToEntity not used for read operations");
  }

  public override async Task<ArticleResponse> FromEntityAsync(Article article, CancellationToken ct = default)
  {
    // Get current user with following relationships if authenticated
    User? currentUser = null;
    var currentUserId = _currentUserService.GetCurrentUserId();
    if (currentUserId.HasValue)
    {
      currentUser = await _userRepository.FirstOrDefaultAsync(
        new UserWithFollowingSpec(currentUserId.Value), ct);
    }

    var isFavorited = currentUser != null && article.FavoritedBy.Any(u => u.Id == currentUser.Id);
    var isFollowing = currentUser?.IsFollowing(article.AuthorId) ?? false;

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
