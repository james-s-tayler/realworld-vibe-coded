using Server.Infrastructure;
using Server.UseCases.Articles.List;
using Server.UseCases.Interfaces;

namespace Server.Web.Articles.List;

public class List(IMediator mediator, IUserContext userContext) : Endpoint<ListArticlesRequest, ArticlesResponse, ArticlesMapper>
{
  public override void Configure()
  {
    Get("/api/articles");
    AuthSchemes(Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme, Microsoft.AspNetCore.Identity.IdentityConstants.BearerScheme);
  }

  public override async Task HandleAsync(ListArticlesRequest request, CancellationToken cancellationToken)
  {
    var userId = userContext.GetRequiredCurrentUserId();

    var result = await mediator.Send(
      new ListArticlesQuery(
        userId,
        request.Author,
        request.Tag,
        request.Favorited,
        request.Limit,
        request.Offset),
      cancellationToken);

    await Send.ResultMapperAsync(
      result,
      async (listResult, ct) => await Map.FromEntityAsync(listResult, ct),
      cancellationToken);
  }
}
