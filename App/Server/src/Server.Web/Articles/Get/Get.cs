using Server.Infrastructure;
using Server.UseCases.Articles.Get;
using Server.UseCases.Interfaces;

namespace Server.Web.Articles.Get;

public class Get(IMediator mediator, IUserContext userContext) : Endpoint<GetArticleRequest, ArticleResponse, ArticleMapper>
{
  public override void Configure()
  {
    Get("/api/articles/{slug}");
    AuthSchemes(Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme, Microsoft.AspNetCore.Identity.IdentityConstants.BearerScheme);
  }

  public override async Task HandleAsync(GetArticleRequest request, CancellationToken cancellationToken)
  {
    var userId = userContext.GetRequiredCurrentUserId();

    var result = await mediator.Send(
      new GetArticleQuery(request.Slug, userId),
      cancellationToken);

    await Send.ResultMapperAsync(
      result,
      async (articleResult, ct) => await Map.FromEntityAsync(articleResult, ct),
      cancellationToken);
  }
}
