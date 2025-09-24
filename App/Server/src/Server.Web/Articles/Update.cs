using System.Security.Claims;
using Server.UseCases.Articles;
using Server.UseCases.Articles.Update;

namespace Server.Web.Articles;

/// <summary>
/// Update article
/// </summary>
/// <remarks>
/// Updates an existing article. Authentication required. User must be the author.
/// </remarks>
public class Update(IMediator _mediator) : Endpoint<UpdateArticleRequest, ArticleResponse>
{
  public override void Configure()
  {
    Put("/api/articles/{slug}");
    AuthSchemes("Token");
    Summary(s =>
    {
      s.Summary = "Update article";
      s.Description = "Updates an existing article. Authentication required. User must be the author.";
    });
  }

  public override async Task HandleAsync(UpdateArticleRequest request, CancellationToken cancellationToken)
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

    var result = await _mediator.Send(new UpdateArticleCommand(
      request.Slug,
      request.Article.Title,
      request.Article.Description,
      request.Article.Body,
      userId), cancellationToken);

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

    if (result.Status == ResultStatus.Forbidden)
    {
      HttpContext.Response.StatusCode = 403;
      HttpContext.Response.ContentType = "application/json";
      var forbiddenJson = System.Text.Json.JsonSerializer.Serialize(new
      {
        errors = new { body = new[] { "You can only update your own articles" } }
      });
      await HttpContext.Response.WriteAsync(forbiddenJson, cancellationToken);
      return;
    }

    HttpContext.Response.StatusCode = 422;
    HttpContext.Response.ContentType = "application/json";
    var errorResponse = System.Text.Json.JsonSerializer.Serialize(new
    {
      errors = new { body = result.Errors.ToArray() }
    });
    await HttpContext.Response.WriteAsync(errorResponse, cancellationToken);
  }
}

public class UpdateArticleRequest
{
  public string Slug { get; set; } = string.Empty;
  public UpdateArticleData Article { get; set; } = new();
}

public class UpdateArticleData
{
  public string Title { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
  public string Body { get; set; } = string.Empty;
}
