using Server.Infrastructure;
using Server.UseCases.Articles.Delete;
using Server.UseCases.Interfaces;

namespace Server.Web.Articles.Delete;

public class Delete(IMediator mediator, IUserContext userContext) : Endpoint<DeleteArticleRequest>
{
  public override void Configure()
  {
    Delete("/api/articles/{slug}");
    AuthSchemes(Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme, Microsoft.AspNetCore.Identity.IdentityConstants.BearerScheme);
  }

  public override async Task HandleAsync(DeleteArticleRequest request, CancellationToken cancellationToken)
  {
    var userId = userContext.GetRequiredCurrentUserId();

    var result = await mediator.Send(
      new DeleteArticleCommand(request.Slug, userId),
      cancellationToken);

    await Send.ResultValueAsync(result, cancellationToken);
  }
}
