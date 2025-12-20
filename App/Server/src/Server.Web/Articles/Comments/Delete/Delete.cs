using Server.Infrastructure;
using Server.UseCases.Articles.Comments.Delete;
using Server.UseCases.Interfaces;

namespace Server.Web.Articles.Comments.Delete;

/// <summary>
/// Delete comment
/// </summary>
/// <remarks>
/// Delete a comment. Authentication required. Only comment author can delete.
/// </remarks>
public class Delete(IMediator mediator, IUserContext userContext) : Endpoint<DeleteCommentRequest>
{
  public override void Configure()
  {
    Delete("/api/articles/{slug}/comments/{id}");
    AuthSchemes("Token", Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme, Microsoft.AspNetCore.Identity.IdentityConstants.BearerScheme);
    Summary(s =>
    {
      s.Summary = "Delete comment";
      s.Description = "Delete a comment. Authentication required. Only comment author can delete.";
    });
  }

  public override async Task HandleAsync(DeleteCommentRequest request, CancellationToken cancellationToken)
  {
    var userId = userContext.GetRequiredCurrentUserId();

    var result = await mediator.Send(new DeleteCommentCommand(request.Slug, request.Id, userId), cancellationToken);

    await Send.ResultValueAsync(result, cancellationToken);
  }
}
