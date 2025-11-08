using Server.Core.Interfaces;
using Server.Infrastructure;
using Server.UseCases.Articles;
using Server.UseCases.Articles.Feed;

namespace Server.Web.Articles.Feed;

/// <summary>
/// Get user's feed
/// </summary>
/// <remarks>
/// Get articles from followed users. Authentication required.
/// </remarks>
public class Feed(IMediator mediator, IUserContext userContext) : Endpoint<FeedRequest, ArticlesResponse, ArticleMapper>
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
    var userId = userContext.GetRequiredCurrentUserId();

    var result = await mediator.Send(new GetFeedQuery(userId, request.Limit, request.Offset), cancellationToken);

    await Send.ResultMapperAsync(result, articles =>
    {
      var articleDtos = articles.Select(article => Map.FromEntity(article).Article).ToList();
      return new ArticlesResponse(articleDtos, articleDtos.Count);
    }, cancellationToken);
  }
}
