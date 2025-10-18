using Server.Core.Interfaces;
using Server.UseCases.Articles.Comments.Delete;
using Server.Web.Infrastructure;

namespace Server.Web.Articles.Comments.Delete;

/// <summary>
/// Delete comment
/// </summary>
/// <remarks>
/// Delete a comment. Authentication required. Only comment author can delete.
/// </remarks>
public class Delete(IMediator _mediator, ICurrentUserService _currentUserService) : Endpoint<DeleteCommentRequest>
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
    var userId = _currentUserService.GetRequiredCurrentUserId();

    var result = await _mediator.Send(new DeleteCommentCommand(request.Slug, request.Id, userId), cancellationToken);

    await Send.ResultAsync(result, _ => new { }, cancellationToken);
  }
}
