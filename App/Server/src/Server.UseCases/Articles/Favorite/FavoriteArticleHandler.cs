using Ardalis.Result;
using Ardalis.SharedKernel;
using Microsoft.Extensions.Logging;
using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Dtos;
using Server.Core.ArticleAggregate.Specifications;
using Server.Core.Interfaces;
using Server.Core.UserAggregate;

namespace Server.UseCases.Articles.Favorite;

public class FavoriteArticleHandler : ICommandHandler<FavoriteArticleCommand, Result<ArticleDto>>
{
  private readonly IRepository<Article> _articleRepository;
  private readonly IRepository<User> _userRepository;
  private readonly ILogger<FavoriteArticleHandler> _logger;

  public FavoriteArticleHandler(
    IRepository<Article> articleRepository,
    IRepository<User> userRepository,
    ILogger<FavoriteArticleHandler> logger)
  {
    _articleRepository = articleRepository;
    _userRepository = userRepository;
    _logger = logger;
  }

  public async Task<Result<ArticleDto>> Handle(FavoriteArticleCommand request, CancellationToken cancellationToken)
  {
    _logger.LogInformation("User {UserId} favoriting article {Slug}", request.UserId, request.Slug);

    var user = await _userRepository
      .FirstOrDefaultAsync(new UserWithFollowingSpec(request.UserId), cancellationToken);

    if (user == null)
    {
      return Result.NotFound("User not found");
    }

    var article = await _articleRepository
      .FirstOrDefaultAsync(new ArticleBySlugWithDetailsSpec(request.Slug), cancellationToken);

    if (article == null)
    {
      return Result.NotFound("Article not found");
    }

    try
    {
      article.AddToFavorites(user);
      await _articleRepository.UpdateAsync(article, cancellationToken);

      var favorited = article.FavoritedBy.Any(u => u.Id == user.Id);
      var following = user.IsFollowing(article.Author);

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
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error favoriting article {Slug} by user {UserId}", request.Slug, request.UserId);
      return Result.Error("An error occurred while favoriting the article");
    }
  }
}
