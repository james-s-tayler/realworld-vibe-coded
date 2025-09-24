using System.Security.Claims;
using Server.UseCases.Articles;
using Server.UseCases.Articles.List;

namespace Server.Web.Articles;

/// <summary>
/// List articles
/// </summary>
/// <remarks>
/// List articles globally. Optional filters for tag, author, favorited user. Authentication optional.
/// </remarks>
public class List(IMediator _mediator) : EndpointWithoutRequest<ArticlesResponse>
{
  public override void Configure()
  {
    Get("/api/articles");
    AllowAnonymous();
    Summary(s =>
    {
      s.Summary = "List articles";
      s.Description = "List articles globally. Optional filters for tag, author, favorited user. Authentication optional.";
    });
  }

  public override async Task HandleAsync(CancellationToken cancellationToken)
  {
    // Get query parameters from the request as strings first to validate
    var tagParam = HttpContext.Request.Query["tag"].FirstOrDefault();
    var authorParam = HttpContext.Request.Query["author"].FirstOrDefault();
    var favoritedParam = HttpContext.Request.Query["favorited"].FirstOrDefault();
    var limitParam = HttpContext.Request.Query["limit"].FirstOrDefault();
    var offsetParam = HttpContext.Request.Query["offset"].FirstOrDefault();

    var errors = new List<string>();

    // Parse and validate limit
    int limit = 20;
    if (!string.IsNullOrEmpty(limitParam))
    {
      if (!int.TryParse(limitParam, out limit))
      {
        errors.Add("limit must be a valid integer");
      }
      else if (limit <= 0)
      {
        errors.Add("limit must be greater than 0");
      }
    }

    // Parse and validate offset
    int offset = 0;
    if (!string.IsNullOrEmpty(offsetParam))
    {
      if (!int.TryParse(offsetParam, out offset))
      {
        errors.Add("offset must be a valid integer");
      }
      else if (offset < 0)
      {
        errors.Add("offset must be greater than or equal to 0");
      }
    }

    // Validate string parameters for empty values
    if (tagParam == "") errors.Add("tag cannot be empty");
    if (authorParam == "") errors.Add("author cannot be empty");
    if (favoritedParam == "") errors.Add("favorited cannot be empty");

    if (errors.Any())
    {
      HttpContext.Response.StatusCode = 422;
      HttpContext.Response.ContentType = "application/json";

      var validationErrorJson = System.Text.Json.JsonSerializer.Serialize(new
      {
        errors = new { body = errors.ToArray() }
      });
      await HttpContext.Response.WriteAsync(validationErrorJson, cancellationToken);
      return;
    }

    // Get current user ID if authenticated
    int? currentUserId = null;
    var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
    if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
    {
      currentUserId = userId;
    }

    var result = await _mediator.Send(new ListArticlesQuery(
      tagParam,
      authorParam,
      favoritedParam,
      limit,
      offset,
      currentUserId), cancellationToken);

    if (result.IsSuccess)
    {
      Response = result.Value;
      return;
    }

    HttpContext.Response.StatusCode = 400;
    HttpContext.Response.ContentType = "application/json";
    var errorJson = System.Text.Json.JsonSerializer.Serialize(new
    {
      errors = new { body = new[] { result.Errors.FirstOrDefault() ?? "Failed to retrieve articles" } }
    });
    await HttpContext.Response.WriteAsync(errorJson, cancellationToken);
  }
}
