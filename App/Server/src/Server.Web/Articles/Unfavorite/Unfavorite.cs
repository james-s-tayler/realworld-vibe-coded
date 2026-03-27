using Server.Infrastructure;
using Server.UseCases.Articles.Unfavorite;
using Server.UseCases.Interfaces;

namespace Server.Web.Articles.Unfavorite;

public class Unfavorite(IMediator mediator, IUserContext userContext) : Endpoint<UnfavoriteRequest, ArticleResponse, ArticleMapper>
{
  public override void Configure()
  {
    Delete("/api/articles/{slug}/favorite");
    AuthSchemes(Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme, Microsoft.AspNetCore.Identity.IdentityConstants.BearerScheme);
  }

  public override async Task HandleAsync(UnfavoriteRequest request, CancellationToken cancellationToken)
  {
    var userId = userContext.GetRequiredCurrentUserId();

    var result = await mediator.Send(
      new UnfavoriteCommand(request.Slug, userId),
      cancellationToken);

    await Send.ResultMapperAsync(
      result,
      async (articleResult, ct) => await Map.FromEntityAsync(articleResult, ct),
      cancellationToken);
  }
}
