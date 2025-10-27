using Server.Core.Interfaces;
using Server.Infrastructure;
using Server.UseCases.Articles.Comments.Delete;

namespace Server.Web.Articles.Comments.Delete;

/// <summary>
/// Delete comment
/// </summary>
/// <remarks>
/// Delete a comment. Authentication required. Only comment author can delete.
/// </remarks>
public class Delete(IMediator _mediator, IUserContext userContext) : Endpoint<DeleteCommentRequest>
{
  public override void Configure()
  {
    Delete("/api/articles/{slug}/comments/{id}");
    AuthSchemes("Token");
    Summary(s =>
    {
      s.Summary = "Delete comment";
      s.Description = "Delete a comment. Authentication required. Only comment author can delete.";
    });
  }

  public override async Task HandleAsync(DeleteCommentRequest request, CancellationToken cancellationToken)
  {
    var userId = userContext.GetRequiredCurrentUserId();

    var result = await _mediator.Send(new DeleteCommentCommand(request.Slug, request.Id, userId), cancellationToken);

    await Send.ResultValueAsync(result, cancellationToken);
  }
}
