using Server.Infrastructure;
using Server.UseCases.Comments.Delete;
using Server.UseCases.Interfaces;

namespace Server.Web.Comments.Delete;

public class Delete(IMediator mediator, IUserContext userContext) : Endpoint<DeleteCommentRequest>
{
  public override void Configure()
  {
    Delete("/api/articles/{slug}/comments/{id}");
    AuthSchemes(Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme, Microsoft.AspNetCore.Identity.IdentityConstants.BearerScheme);
  }

  public override async Task HandleAsync(DeleteCommentRequest request, CancellationToken cancellationToken)
  {
    var userId = userContext.GetRequiredCurrentUserId();

    var result = await mediator.Send(
      new DeleteCommentCommand(request.Slug, request.Id, userId),
      cancellationToken);

    await Send.ResultValueAsync(result, cancellationToken);
  }
}
