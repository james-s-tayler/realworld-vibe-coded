using System.Security.Claims;
using Server.Core.ArticleAggregate.Dtos;
using Server.UseCases.Articles.Favorite;

namespace Server.Web.Articles;

/// <summary>
/// Favorite an article
/// </summary>
/// <remarks>
/// Favorite an article. Authentication required.
/// </remarks>
public class FavoriteArticle(IMediator _mediator) : EndpointWithoutRequest<FavoriteArticleResponse>
{
  public override void Configure()
  {
    Post("/api/articles/{slug}/favorite");
    AuthSchemes("Token");
    Summary(s =>
    {
      s.Summary = "Favorite an article";
      s.Description = "Favorite an article. Authentication required.";
    });
  }

  public override async Task HandleAsync(CancellationToken cancellationToken)
  {
    var slug = Route<string>("slug") ?? string.Empty;
    var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);

    if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
    {
      HttpContext.Response.StatusCode = 401;
      HttpContext.Response.ContentType = "application/json";
      var unauthorizedJson = System.Text.Json.JsonSerializer.Serialize(new
      {
        errors = new { body = new[] { "Unauthorized" } }
      });
      await HttpContext.Response.WriteAsync(unauthorizedJson, cancellationToken);
      return;
    }

    var result = await _mediator.Send(new FavoriteArticleCommand(userId, slug), cancellationToken);

    if (result.IsSuccess)
    {
      Response = new FavoriteArticleResponse { Article = result.Value };
      return;
    }

    if (result.Status == Ardalis.Result.ResultStatus.NotFound)
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
    var errorJson = System.Text.Json.JsonSerializer.Serialize(new
    {
      errors = new { body = new[] { result.Errors.FirstOrDefault() ?? "Failed to favorite article" } }
    });
    await HttpContext.Response.WriteAsync(errorJson, cancellationToken);
  }
}

public class FavoriteArticleResponse
{
  public ArticleDto Article { get; set; } = default!;
}
