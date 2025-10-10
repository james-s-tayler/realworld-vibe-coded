using Server.Core.Interfaces;
using Server.UseCases.Articles;
using Server.UseCases.Articles.Get;

namespace Server.Web.Articles;

/// <summary>
/// Get article by slug
/// </summary>
/// <remarks>
/// Gets a single article by its slug. Authentication optional.
/// </remarks>
public class Get(IMediator _mediator, ICurrentUserService _currentUserService) : EndpointWithoutRequest<ArticleResponse>
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

  public override async Task HandleAsync(CancellationToken cancellationToken)
  {
    // Get slug from route parameter
    var slug = Route<string>("slug") ?? string.Empty;

    // Get current user ID if authenticated
    var currentUserId = _currentUserService.GetCurrentUserId();

    var result = await _mediator.Send(new GetArticleQuery(slug, currentUserId), cancellationToken);

    if (result.IsSuccess)
    {
      // Use FastEndpoints mapper to convert Article to ArticleResponse
      var mapper = Resolve<ArticleMapper>();
      Response = await mapper.FromEntityAsync(result.Value, cancellationToken);
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


