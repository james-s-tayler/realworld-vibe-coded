using FluentValidation.Results;
using Server.Core.Interfaces;
using Server.UseCases.Articles;
using Server.UseCases.Articles.Feed;
using Server.Web.Infrastructure;

namespace Server.Web.Articles.Feed;

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

    var limitParam = HttpContext.Request.Query["limit"].FirstOrDefault();
    var offsetParam = HttpContext.Request.Query["offset"].FirstOrDefault();

    // Parse and validate limit
    int limit = 20;
    if (!string.IsNullOrEmpty(limitParam))
    {
      if (!int.TryParse(limitParam, out limit))
      {
        AddError(new ValidationFailure("limit", "limit must be a valid integer"));
      }
      else if (limit <= 0)
      {
        AddError(new ValidationFailure("limit", "limit must be greater than 0"));
      }
    }

    // Parse and validate offset
    int offset = 0;
    if (!string.IsNullOrEmpty(offsetParam))
    {
      if (!int.TryParse(offsetParam, out offset))
      {
        AddError(new ValidationFailure("offset", "offset must be a valid integer"));
      }
      else if (offset < 0)
      {
        AddError(new ValidationFailure("offset", "offset must be greater than or equal to 0"));
      }
    }

    ThrowIfAnyErrors();

    var result = await _mediator.Send(new GetFeedQuery(userId, validation.Limit, validation.Offset), cancellationToken);

    await Send.ResultAsync(result, articles =>
    {
      var articleDtos = articles.Select(article => Map.FromEntity(article).Article).ToList();
      return new ArticlesResponse(articleDtos, articleDtos.Count);
    }, cancellationToken);
  }
}
