using System.Security.Claims;
using Server.Core.ArticleAggregate.Dtos;
using Server.UseCases.Articles.GetBySlug;

namespace Server.Web.Articles;

/// <summary>
/// Get article by slug
/// </summary>
/// <remarks>
/// Get a single article by its slug. Authentication optional.
/// </remarks>
public class GetArticle(IMediator _mediator) : EndpointWithoutRequest<GetArticleResponse>
{
  public override void Configure()
  {
    Get("/api/articles/{slug}");
    AllowAnonymous();
    Summary(s =>
    {
      s.Summary = "Get article by slug";
      s.Description = "Get a single article by its slug. Authentication optional.";
    });
  }

  public override async Task HandleAsync(CancellationToken cancellationToken)
  {
    var slug = Route<string>("slug") ?? string.Empty;
    
    // Get current user ID if authenticated
    int? currentUserId = null;
    var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
    if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
    {
      currentUserId = userId;
    }

    var result = await _mediator.Send(new GetArticleBySlugQuery(slug, currentUserId), cancellationToken);

    if (result.IsSuccess)
    {
      Response = new GetArticleResponse { Article = result.Value };
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
      errors = new { body = new[] { result.Errors.FirstOrDefault() ?? "Failed to get article" } }
    });
    await HttpContext.Response.WriteAsync(errorJson, cancellationToken);
  }
}

public class GetArticleResponse
{
  public ArticleDto Article { get; set; } = default!;
}