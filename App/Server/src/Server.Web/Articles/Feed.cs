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
public class Feed(IMediator _mediator, ICurrentUserService _currentUserService) : EndpointWithoutRequest<ArticlesResponse, ArticleMapper>
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

  public override async Task HandleAsync(CancellationToken cancellationToken)
  {
    // Get current user ID from service
    var userId = _currentUserService.GetRequiredCurrentUserId();

    var validation = QueryParameterValidator.ValidateFeedParameters(HttpContext.Request);

    if (!validation.IsValid)
    {
      await Send.ResponseAsync<ConduitErrorResponse>(new ConduitErrorResponse
      {
        Errors = new ConduitErrorBody { Body = validation.Errors.ToArray() }
      }, 422);
      return;
    }

    var result = await _mediator.Send(new GetFeedQuery(userId, validation.Limit, validation.Offset), cancellationToken);

    if (result.IsSuccess)
    {
      // Map each Article entity to ArticleDto using FastEndpoints mapper
      var articles = result.Value.ToList();
      var articleDtos = articles.Select(article => Map.FromEntity(article).Article).ToList();
      Response = new ArticlesResponse(articleDtos, articleDtos.Count);
      return;
    }

    await Send.ResponseAsync<ConduitErrorResponse>(new ConduitErrorResponse
    {
      Errors = new ConduitErrorBody { Body = result.Errors.ToArray() }
    }, 400);
  }
}
