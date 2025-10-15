using Server.Core.Interfaces;
using Server.UseCases.Articles.Comments.Delete;
using Server.Web.Infrastructure;

namespace Server.Web.Articles.Comments;

/// <summary>
/// Delete comment
/// </summary>
/// <remarks>
/// Delete a comment. Authentication required. Only comment author can delete.
/// </remarks>
public class Delete(IMediator _mediator, ICurrentUserService _currentUserService) : EndpointWithoutRequest
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

  public override async Task HandleAsync(CancellationToken cancellationToken)
  {
    var userId = _currentUserService.GetRequiredCurrentUserId();

    // Get parameters from route
    var slug = Route<string>("slug") ?? string.Empty;
    var commentIdStr = Route<string>("id") ?? string.Empty;

    if (string.IsNullOrEmpty(slug))
    {
      await Send.ValidationErrorAsync(new[] { "Article slug is required" }, cancellationToken);
      return;
    }

    if (!int.TryParse(commentIdStr, out var commentId))
    {
      await Send.ValidationErrorAsync(new[] { "id is invalid" }, cancellationToken);
      return;
    }

    var result = await _mediator.Send(new DeleteCommentCommand(slug, commentId, userId), cancellationToken);

    await Send.ResultAsync(result, _ => new { }, cancellationToken);
  }
}
