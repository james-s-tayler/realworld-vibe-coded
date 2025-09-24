using System.Security.Claims;
using Server.UseCases.Articles;
using Server.UseCases.Articles.Feed;

namespace Server.Web.Articles;

/// <summary>
/// Get user's feed
/// </summary>
/// <remarks>
/// Get articles from followed users. Authentication required.
/// </remarks>
public class Feed(IMediator _mediator) : EndpointWithoutRequest<ArticlesResponse>
{
  public override void Configure()
  {
    Get("/api/articles/feed");
    AuthSchemes("Token");
    Summary(s =>
    {
      s.Summary = "Get user's feed";
      s.Description = "Get articles from followed users. Authentication required.";
    });
  }

  public override async Task HandleAsync(CancellationToken cancellationToken)
  {
    // Get query parameters from the request
    var limit = Query<int?>("limit", false) ?? 20;
    var offset = Query<int?>("offset", false) ?? 0;

    // Get current user ID from claims
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

    // Validate input parameters
    if (limit <= 0 || offset < 0)
    {
      HttpContext.Response.StatusCode = 422;
      HttpContext.Response.ContentType = "application/json";
      
      var errors = new List<string>();
      if (limit <= 0) errors.Add("limit must be greater than 0");
      if (offset < 0) errors.Add("offset must be greater than or equal to 0");
      
      var validationErrorJson = System.Text.Json.JsonSerializer.Serialize(new
      {
        errors = new { body = errors.ToArray() }
      });
      await HttpContext.Response.WriteAsync(validationErrorJson, cancellationToken);
      return;
    }

    var result = await _mediator.Send(new GetFeedQuery(userId, limit, offset), cancellationToken);

    if (result.IsSuccess)
    {
      Response = result.Value;
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