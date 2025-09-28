using FastEndpoints;
using MediatR;
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
      HttpContext.Response.StatusCode = 422;
      HttpContext.Response.ContentType = "application/json";
      var slugErrorJson = System.Text.Json.JsonSerializer.Serialize(new
      {
        errors = new { body = new[] { "Article slug is required" } }
      });
      await HttpContext.Response.WriteAsync(slugErrorJson);
      return;
    }

    if (!int.TryParse(commentIdStr, out var commentId))
    {
      HttpContext.Response.StatusCode = 422;
      HttpContext.Response.ContentType = "application/json";
      var idErrorJson = System.Text.Json.JsonSerializer.Serialize(new
      {
        errors = new { body = new[] { "id is invalid" } }
      });
      await HttpContext.Response.WriteAsync(idErrorJson);
      return;
    }

    var result = await _mediator.Send(new DeleteCommentCommand(slug, commentId, userId), cancellationToken);

    if (result.IsSuccess)
    {
      HttpContext.Response.StatusCode = 200;
      HttpContext.Response.ContentType = "application/json";
      await HttpContext.Response.WriteAsync("{}", cancellationToken);
      return;
    }

    if (result.Status == Ardalis.Result.ResultStatus.NotFound)
    {
      HttpContext.Response.StatusCode = 422;
      HttpContext.Response.ContentType = "application/json";
      var errorJson = System.Text.Json.JsonSerializer.Serialize(new
      {
        errors = new { body = result.Errors.ToArray() }
      });
      await HttpContext.Response.WriteAsync(errorJson);
      return;
    }

    if (result.Status == Ardalis.Result.ResultStatus.Forbidden)
    {
      HttpContext.Response.StatusCode = 403;
      HttpContext.Response.ContentType = "application/json";
      var errorJson = System.Text.Json.JsonSerializer.Serialize(new
      {
        errors = new { body = result.Errors.ToArray() }
      });
      await HttpContext.Response.WriteAsync(errorJson);
      return;
    }

    HttpContext.Response.StatusCode = 422;
    HttpContext.Response.ContentType = "application/json";
    var defaultErrorJson = System.Text.Json.JsonSerializer.Serialize(new
    {
      errors = new { body = result.Errors.ToArray() }
    });
    await HttpContext.Response.WriteAsync(defaultErrorJson);
  }
}
