using Ardalis.Result;
using Ardalis.SharedKernel;
using Microsoft.Extensions.Logging;
using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Dtos;
using Server.Core.ArticleAggregate.Specifications;
using Server.Core.Interfaces;
using Server.Core.UserAggregate;

namespace Server.UseCases.Articles.GetBySlug;

public class GetArticleBySlugHandler : IQueryHandler<GetArticleBySlugQuery, Result<ArticleDto>>
{
  private readonly IRepository<Article> _articleRepository;
  private readonly IRepository<User> _userRepository;
  private readonly ILogger<GetArticleBySlugHandler> _logger;

  public GetArticleBySlugHandler(
    IRepository<Article> articleRepository,
    IRepository<User> userRepository,
    ILogger<GetArticleBySlugHandler> logger)
  {
    _articleRepository = articleRepository;
    _userRepository = userRepository;
    _logger = logger;
  }

  public async Task<Result<ArticleDto>> Handle(GetArticleBySlugQuery request, CancellationToken cancellationToken)
  {
    _logger.LogInformation("Getting article by slug: {Slug}", request.Slug);

    var article = await _articleRepository
      .FirstOrDefaultAsync(new ArticleBySlugWithDetailsSpec(request.Slug), cancellationToken);

    if (article == null)
    {
      return Result.NotFound("Article not found");
    }

    // Get current user if provided for favorited/following checks
    User? currentUser = null;
    if (request.CurrentUserId.HasValue)
    {
      // Load user with following relationships
      currentUser = await _userRepository
        .FirstOrDefaultAsync(new UserWithFollowingSpec(request.CurrentUserId.Value), cancellationToken);
    }

    var favorited = currentUser != null && article.FavoritedBy.Any(u => u.Id == currentUser.Id);
    var following = currentUser != null && currentUser.IsFollowing(article.Author);

    return Result.Success(new ArticleDto(
      article.Slug,
      article.Title,
      article.Description,
      article.Body,
      article.Tags.Select(t => t.Name).ToList(),
      DateTime.SpecifyKind(article.CreatedAt, DateTimeKind.Utc),
      DateTime.SpecifyKind(article.UpdatedAt, DateTimeKind.Utc),
      favorited,
      article.FavoritesCount,
      new AuthorDto(
        article.Author.Username,
        article.Author.Bio ?? string.Empty,
        article.Author.Image,
        following
      )
    ));
  }
}
