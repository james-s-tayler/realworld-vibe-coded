using Server.Core.ArticleAggregate.Dtos;
using Server.Infrastructure;
using Server.SharedKernel.Pagination;
using Server.UseCases.Articles.Feed;
using Server.UseCases.Interfaces;

namespace Server.Web.Articles.Feed;

/// <summary>
/// Get user's feed
/// </summary>
/// <remarks>
/// Get articles from followed users. Authentication required.
/// </remarks>
public class Feed(IMediator mediator, IUserContext userContext)
  : Endpoint<FeedRequest, PaginatedResponse<ArticleDto>, FeedMapper>
{
  public override void Configure()
  {
    Get("/api/articles/feed");
    AuthSchemes(Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme, Microsoft.AspNetCore.Identity.IdentityConstants.BearerScheme);
    Summary(s =>
    {
      s.Summary = "Get user's feed";
      s.Description = "Get articles from followed users. Authentication required.";
    });
  }

  public override async Task HandleAsync(FeedRequest request, CancellationToken cancellationToken)
  {
    var userId = userContext.GetRequiredCurrentUserId();

    var result = await mediator.Send(
      new GetFeedQuery(
        userId,
        request.Limit,
        request.Offset),
      cancellationToken);

    await Send.ResultMapperAsync(result, Map.FromEntityAsync, cancellationToken);
  }
}
