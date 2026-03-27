using Server.Infrastructure;
using Server.UseCases.Articles.Favorite;
using Server.UseCases.Interfaces;

namespace Server.Web.Articles.Favorite;

public class Favorite(IMediator mediator, IUserContext userContext) : Endpoint<FavoriteRequest, ArticleResponse, ArticleMapper>
{
  public override void Configure()
  {
    Post("/api/articles/{slug}/favorite");
    AuthSchemes(Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme, Microsoft.AspNetCore.Identity.IdentityConstants.BearerScheme);
  }

  public override async Task HandleAsync(FavoriteRequest request, CancellationToken cancellationToken)
  {
    var userId = userContext.GetRequiredCurrentUserId();

    var result = await mediator.Send(
      new FavoriteCommand(request.Slug, userId),
      cancellationToken);

    await Send.ResultMapperAsync(
      result,
      async (articleResult, ct) => await Map.FromEntityAsync(articleResult, ct),
      cancellationToken);
  }
}
