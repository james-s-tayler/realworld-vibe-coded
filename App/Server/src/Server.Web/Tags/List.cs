using Server.UseCases.Tags;
using Server.UseCases.Tags.List;

namespace Server.Web.Tags;

/// <summary>
/// Get all tags
/// </summary>
/// <remarks>
/// Get all tags used in articles. No authentication required.
/// </remarks>
public class List(IMediator _mediator) : EndpointWithoutRequest<TagsResponse>
{
  public override void Configure()
  {
    Get("/api/tags");
    AllowAnonymous();
    Summary(s =>
    {
      s.Summary = "Get all tags";
      s.Description = "Get all tags used in articles. No authentication required.";
    });
  }

  public override async Task HandleAsync(CancellationToken cancellationToken)
  {
    // TEMPORARY: Force failure for CI testing
    HttpContext.Response.StatusCode = 500;
    HttpContext.Response.ContentType = "application/json";
    var errorJson = System.Text.Json.JsonSerializer.Serialize(new
    {
      errors = new { body = new[] { "Temporary failure for CI testing" } }
    });
    await HttpContext.Response.WriteAsync(errorJson, cancellationToken);
    return;

    #pragma warning disable CS0162 // Unreachable code detected
    // Original code commented out
    var result = await _mediator.Send(new ListTagsQuery(), cancellationToken);

    if (result.IsSuccess)
    {
      Response = result.Value;
      return;
    }

    HttpContext.Response.StatusCode = 400;
    HttpContext.Response.ContentType = "application/json";
    var errorJsonOriginal = System.Text.Json.JsonSerializer.Serialize(new
    {
      errors = new { body = new[] { result.Errors.FirstOrDefault() ?? "Failed to retrieve tags" } }
    });
    await HttpContext.Response.WriteAsync(errorJsonOriginal, cancellationToken);
    #pragma warning restore CS0162 // Unreachable code detected
  }
}
