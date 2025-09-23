using Ardalis.Result;
using Ardalis.SharedKernel;
using Microsoft.Extensions.Logging;
using Server.Core.ArticleAggregate;
using Server.Core.Interfaces;
using Server.Core.UserAggregate;

namespace Server.UseCases.Articles.List;

public class ListArticlesHandler(
  IRepository<Article> _articleRepository,
  IRepository<User> _userRepository,
  ILogger<ListArticlesHandler> _logger)
  : IQueryHandler<ListArticlesQuery, Result<ListArticlesResult>>
{
  public async Task<Result<ListArticlesResult>> Handle(
    ListArticlesQuery request,
    CancellationToken cancellationToken)
  {
    _logger.LogInformation("Handling {Query}", nameof(ListArticlesQuery));
    _logger.LogInformation("Property {PropertyName} : {PropertyValue}", nameof(request.Tag), request.Tag);
    _logger.LogInformation("Property {PropertyName} : {PropertyValue}", nameof(request.Author), request.Author);
    _logger.LogInformation("Property {PropertyName} : {PropertyValue}", nameof(request.Favorited), request.Favorited);
    _logger.LogInformation("Property {PropertyName} : {PropertyValue}", nameof(request.Limit), request.Limit);
    _logger.LogInformation("Property {PropertyName} : {PropertyValue}", nameof(request.Offset), request.Offset);

    try
    {
      List<Article> articles;
      int totalCount;

      // Handle different filter cases
      if (!string.IsNullOrEmpty(request.Author))
      {
        // Find user by username
        var authorUser = await _userRepository
          .FirstOrDefaultAsync(new UserByUsernameSpec(request.Author), cancellationToken);

        if (authorUser == null)
        {
          // Author doesn't exist, return empty result
          return Result.Success(new ListArticlesResult
          {
            Articles = new List<ArticleDto>(),
            ArticlesCount = 0
          });
        }

        // Get articles by author
        articles = await _articleRepository
          .ListAsync(new ArticlesByAuthorSpec(authorUser.Id, request.Offset, request.Limit), cancellationToken);
        totalCount = await _articleRepository
          .CountAsync(new ArticleCountByAuthorSpec(authorUser.Id), cancellationToken);
      }
      else if (!string.IsNullOrEmpty(request.Tag))
      {
        // Get all articles and filter by tag in memory (since DB is empty, this is efficient)
        var allArticles = await _articleRepository
          .ListAsync(new ArticlesSpec(0, 1000), cancellationToken); // Get reasonable max

        articles = allArticles
          .Where(a => a.TagList.Contains(request.Tag))
          .Skip(request.Offset)
          .Take(request.Limit)
          .ToList();

        totalCount = allArticles.Count(a => a.TagList.Contains(request.Tag));
      }
      else if (!string.IsNullOrEmpty(request.Favorited))
      {
        // For now, since we don't have favorites functionality, return empty result
        return Result.Success(new ListArticlesResult
        {
          Articles = new List<ArticleDto>(),
          ArticlesCount = 0
        });
      }
      else
      {
        // Get all articles
        articles = await _articleRepository
          .ListAsync(new ArticlesSpec(request.Offset, request.Limit), cancellationToken);
        totalCount = await _articleRepository
          .CountAsync(new ArticleCountSpec(), cancellationToken);
      }

      // Convert to DTOs - for now we'll return empty articles since we don't have complete user mapping yet
      var articleDtos = new List<ArticleDto>();

      // TODO: Once we have articles and proper user mapping, convert articles to DTOs here

      var result = new ListArticlesResult
      {
        Articles = articleDtos,
        ArticlesCount = totalCount
      };

      _logger.LogInformation("Found {ArticleCount} articles", totalCount);
      return Result.Success(result);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error handling {Query}", nameof(ListArticlesQuery));
      return Result.Error("An error occurred while retrieving articles");
    }
  }
}
