using Server.Infrastructure;
using Server.UseCases.Articles.Feed;
using Server.UseCases.Interfaces;

namespace Server.Web.Articles.Feed;

public class Feed(IMediator mediator, IUserContext userContext) : Endpoint<FeedArticlesRequest, ArticlesResponse, ArticlesMapper>
{
  public override void Configure()
  {
    Get("/api/articles/feed");
    AuthSchemes(Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme, Microsoft.AspNetCore.Identity.IdentityConstants.BearerScheme);
  }

  public override async Task HandleAsync(FeedArticlesRequest request, CancellationToken cancellationToken)
  {
    var userId = userContext.GetRequiredCurrentUserId();

    var result = await mediator.Send(
      new FeedArticlesQuery(userId, request.Limit, request.Offset),
      cancellationToken);

    await Send.ResultMapperAsync(
      result,
      async (listResult, ct) => await Map.FromEntityAsync(listResult, ct),
      cancellationToken);
  }
}
