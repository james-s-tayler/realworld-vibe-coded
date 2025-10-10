using Server.Core.Interfaces;
using Server.UseCases.Articles;
using Server.UseCases.Articles.Unfavorite;

namespace Server.Web.Articles;

/// <summary>
/// Unfavorite article
/// </summary>
/// <remarks>
/// Remove article from favorites. Authentication required.
/// </remarks>
public class Unfavorite(IMediator _mediator, ICurrentUserService _currentUserService) : EndpointWithoutRequest<ArticleResponse, ArticleMapper>
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

  public override async Task HandleAsync(CancellationToken cancellationToken)
  {
    // Get slug from route parameter
    var slug = Route<string>("slug") ?? string.Empty;

    var userId = _currentUserService.GetRequiredCurrentUserId();

    var result = await _mediator.Send(new UnfavoriteArticleCommand(slug, userId, userId), cancellationToken);

    if (result.IsSuccess)
    {
      // Use FastEndpoints Map.FromEntityAsync to convert Article to ArticleResponse
      Response = await Map.FromEntityAsync(result.Value, cancellationToken);
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
