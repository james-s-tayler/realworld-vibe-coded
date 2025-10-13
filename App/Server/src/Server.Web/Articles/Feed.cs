using Server.Core.Interfaces;
using Server.UseCases.Articles;
using Server.UseCases.Articles.Feed;

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
      await SendAsync(new
      {
        errors = new { body = validation.Errors.ToArray() }
      }, 422, cancellationToken);
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

    await SendAsync(new
    {
      errors = new { body = result.Errors.ToArray() }
    }, 400, cancellationToken);
  }
}
