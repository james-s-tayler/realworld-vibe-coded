using System.Security.Claims;
using Server.UseCases.Articles.Delete;

namespace Server.Web.Articles;

/// <summary>
/// Delete article
/// </summary>
/// <remarks>
/// Deletes an existing article. Authentication required. User must be the author.
/// </remarks>
public class Delete(IMediator _mediator) : EndpointWithoutRequest
{
  public override void Configure()
  {
    Delete("/api/articles/{slug}");
    AuthSchemes("Token");
    Summary(s =>
    {
      s.Summary = "Delete article";
      s.Description = "Deletes an existing article. Authentication required. User must be the author.";
    });
  }

  public override async Task HandleAsync(CancellationToken cancellationToken)
  {
    var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);

    if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
    {
      HttpContext.Response.StatusCode = 401;
      HttpContext.Response.ContentType = "application/json";
      var errorJson = System.Text.Json.JsonSerializer.Serialize(new
      {
        errors = new { body = new[] { "Unauthorized" } }
      });
      await HttpContext.Response.WriteAsync(errorJson, cancellationToken);
      return;
    }

    // Get slug from route
    var slug = Route<string>("slug") ?? string.Empty;

    var result = await _mediator.Send(new DeleteArticleCommand(slug, userId), cancellationToken);

    if (result.IsSuccess)
    {
      HttpContext.Response.StatusCode = 204;
      return;
    }

    if (result.Status == ResultStatus.NotFound)
    {
      HttpContext.Response.StatusCode = 404;
      HttpContext.Response.ContentType = "application/json";
      var notFoundJson = System.Text.Json.JsonSerializer.Serialize(new
      {
        errors = new { body = new[] { "Article not found" } }
      });
      await HttpContext.Response.WriteAsync(notFoundJson, cancellationToken);
      return;
    }

    if (result.Status == ResultStatus.Forbidden)
    {
      HttpContext.Response.StatusCode = 403;
      HttpContext.Response.ContentType = "application/json";
      var forbiddenJson = System.Text.Json.JsonSerializer.Serialize(new
      {
        errors = new { body = new[] { "You can only delete your own articles" } }
      });
      await HttpContext.Response.WriteAsync(forbiddenJson, cancellationToken);
      return;
    }

    HttpContext.Response.StatusCode = 400;
    HttpContext.Response.ContentType = "application/json";
    var errorResponse = System.Text.Json.JsonSerializer.Serialize(new
    {
      errors = new { body = result.Errors.ToArray() }
    });
    await HttpContext.Response.WriteAsync(errorResponse, cancellationToken);
  }
}
