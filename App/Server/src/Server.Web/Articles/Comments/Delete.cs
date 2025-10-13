using Server.Core.Interfaces;
using Server.UseCases.Articles.Comments.Delete;

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
      await SendAsync(new
      {
        errors = new { body = new[] { "Article slug is required" } }
      }, 422, cancellationToken);
      return;
    }

    if (!int.TryParse(commentIdStr, out var commentId))
    {
      await SendAsync(new
      {
        errors = new { body = new[] { "id is invalid" } }
      }, 422, cancellationToken);
      return;
    }

    var result = await _mediator.Send(new DeleteCommentCommand(slug, commentId, userId), cancellationToken);

    if (result.IsSuccess)
    {
      await SendAsync(new { }, 200, cancellationToken);
      return;
    }

    if (result.Status == Ardalis.Result.ResultStatus.NotFound)
    {
      await SendAsync(new
      {
        errors = new { body = result.Errors.ToArray() }
      }, 422, cancellationToken);
      return;
    }

    if (result.Status == Ardalis.Result.ResultStatus.Forbidden)
    {
      await SendAsync(new
      {
        errors = new { body = result.Errors.ToArray() }
      }, 403, cancellationToken);
      return;
    }

    await SendAsync(new
    {
      errors = new { body = result.Errors.ToArray() }
    }, 422, cancellationToken);
  }
}
