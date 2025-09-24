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
    // Get query parameters from the request
    var tag = Query<string?>("tag", false);
    var author = Query<string?>("author", false);
    var favorited = Query<string?>("favorited", false);
    var limit = Query<int?>("limit", false) ?? 20;
    var offset = Query<int?>("offset", false) ?? 0;

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

    // Get current user ID if authenticated
    int? currentUserId = null;
    var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
    if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
    {
      currentUserId = userId;
    }

    var result = await _mediator.Send(new ListArticlesQuery(
      tag,
      author,
      favorited,
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
