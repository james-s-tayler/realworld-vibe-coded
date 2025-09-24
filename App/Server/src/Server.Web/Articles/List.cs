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
    var validation = QueryParameterValidator.ValidateListArticlesParameters(HttpContext.Request);

    if (!validation.IsValid)
    {
      HttpContext.Response.StatusCode = 422;
      HttpContext.Response.ContentType = "application/json";

      var validationErrorJson = System.Text.Json.JsonSerializer.Serialize(new
      {
        errors = new { body = validation.Errors.ToArray() }
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
      validation.Tag,
      validation.Author,
      validation.Favorited,
      validation.Limit,
      validation.Offset,
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
