using System.Security.Claims;
using Server.UseCases.Articles.Unfavorite;
using Server.UseCases.Articles;

namespace Server.Web.Articles;

/// <summary>
/// Unfavorite article
/// </summary>
/// <remarks>
/// Remove article from favorites. Authentication required.
/// </remarks>
public class Unfavorite(IMediator _mediator) : Endpoint<UnfavoriteArticleRequest, ArticleResponse>
{
  public override void Configure()
  {
    Delete("/api/articles/{slug}/favorite");
    AuthSchemes("Token");
    Summary(s =>
    {
      s.Summary = "Unfavorite article";
      s.Description = "Remove article from favorites. Authentication required.";
    });
  }

  public override async Task HandleAsync(UnfavoriteArticleRequest request, CancellationToken cancellationToken)
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

    var result = await _mediator.Send(new UnfavoriteArticleCommand(request.Slug, userId), cancellationToken);

    if (result.IsSuccess)
    {
      Response = result.Value;
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

    HttpContext.Response.StatusCode = 400;
    HttpContext.Response.ContentType = "application/json";
    var errorResponse = System.Text.Json.JsonSerializer.Serialize(new
    {
      errors = new { body = result.Errors.ToArray() }
    });
    await HttpContext.Response.WriteAsync(errorResponse, cancellationToken);
  }
}

public class UnfavoriteArticleRequest
{
  public string Slug { get; set; } = string.Empty;
}