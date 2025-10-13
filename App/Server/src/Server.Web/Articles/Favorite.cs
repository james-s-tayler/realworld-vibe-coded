using Server.Core.Interfaces;
using Server.UseCases.Articles;
using Server.UseCases.Articles.Favorite;

namespace Server.Web.Articles;

public class FavoriteArticleRequest
{
  [RouteParam]
  public string Slug { get; set; } = string.Empty;
}

/// <summary>
/// Favorite article
/// </summary>
/// <remarks>
/// Add article to favorites. Authentication required.
/// </remarks>
public class Favorite(IMediator _mediator, ICurrentUserService _currentUserService) : Endpoint<FavoriteArticleRequest, ArticleResponse, ArticleMapper>
{
  public override void Configure()
  {
    Post("/api/articles/{slug}/favorite");
    AuthSchemes("Token");
    Summary(s =>
    {
      s.Summary = "Favorite article";
      s.Description = "Add article to favorites. Authentication required.";
    });
  }

  public override async Task HandleAsync(FavoriteArticleRequest request, CancellationToken cancellationToken)
  {
    var slug = request.Slug;

    var userId = _currentUserService.GetRequiredCurrentUserId();

    var result = await _mediator.Send(new FavoriteArticleCommand(slug, userId, userId), cancellationToken);

    if (result.IsSuccess)
    {
      // Use FastEndpoints mapper to convert entity to response DTO
      Response = Map.FromEntity(result.Value);
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
