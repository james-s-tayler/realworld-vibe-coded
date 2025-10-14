using Server.Core.Interfaces;
using Server.UseCases.Articles;
using Server.UseCases.Articles.Feed;
using Server.Web.Infrastructure;

namespace Server.Web.Articles;

/// <summary>
/// Get user's feed
/// </summary>
/// <remarks>
/// Get articles from followed users. Authentication required.
/// </remarks>
public class Feed(IMediator _mediator, ICurrentUserService _currentUserService) : Endpoint<FeedRequest, ArticlesResponse, ArticleMapper>
{
  public override void Configure()
  {
    Get("/api/articles/feed");
    AuthSchemes("Token");
    Summary(s =>
    {
      s.Summary = "Get user's feed";
      s.Description = "Get articles from followed users. Authentication required.";
    });
  }

  public override async Task HandleAsync(FeedRequest request, CancellationToken cancellationToken)
  {
    // Get current user ID from service
    var userId = _currentUserService.GetRequiredCurrentUserId();

    // Parse query parameters with defaults
    var limit = string.IsNullOrEmpty(request.Limit) ? 20 : int.Parse(request.Limit);
    var offset = string.IsNullOrEmpty(request.Offset) ? 0 : int.Parse(request.Offset);

    var result = await _mediator.Send(new GetFeedQuery(userId, limit, offset), cancellationToken);

    await this.SendAsync(result, articles =>
    {
      var articleDtos = articles.Select(article => Map.FromEntity(article).Article).ToList();
      return new ArticlesResponse(articleDtos, articleDtos.Count);
    }, cancellationToken);
  }
}

public class FeedRequest
{
  public string? Limit { get; set; }
  public string? Offset { get; set; }
}
