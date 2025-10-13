using Server.Core.Interfaces;
using Server.UseCases.Articles;
using Server.UseCases.Articles.Get;

namespace Server.Web.Articles;

public class GetArticleRequest
{
  [RouteParam]
  public string Slug { get; set; } = string.Empty;
}

/// <summary>
/// Get article by slug
/// </summary>
/// <remarks>
/// Gets a single article by its slug. Authentication optional.
/// </remarks>
public class Get(IMediator _mediator, ICurrentUserService _currentUserService) : Endpoint<GetArticleRequest, ArticleResponse, ArticleMapper>
{
  public override void Configure()
  {
    Get("/api/articles/{slug}");
    AllowAnonymous();
    Summary(s =>
    {
      s.Summary = "Get article by slug";
      s.Description = "Gets a single article by its slug. Authentication optional.";
    });
  }

  public override async Task HandleAsync(GetArticleRequest request, CancellationToken cancellationToken)
  {
    var slug = request.Slug;

    // Get current user ID if authenticated
    var currentUserId = _currentUserService.GetCurrentUserId();

    var result = await _mediator.Send(new GetArticleQuery(slug, currentUserId), cancellationToken);

    if (result.IsSuccess)
    {
      // Use FastEndpoints mapper to convert entity to response DTO
      Response = Map.FromEntity(result.Value);
      return;
    }

    HttpContext.Response.StatusCode = 404;
    HttpContext.Response.ContentType = "application/json";
    var errorJson = System.Text.Json.JsonSerializer.Serialize(new
    {
      errors = new { body = new[] { "Article not found" } }
    });
    await HttpContext.Response.WriteAsync(errorJson, cancellationToken);
  }
}


