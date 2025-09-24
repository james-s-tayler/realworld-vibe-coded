using Server.UseCases.Articles;
using Server.UseCases.Articles.Get;

namespace Server.Web.Articles;

/// <summary>
/// Get article by slug
/// </summary>
/// <remarks>
/// Gets a single article by its slug. Authentication optional.
/// </remarks>
public class Get(IMediator _mediator) : EndpointWithoutRequest<ArticleResponse>
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

    var result = await _mediator.Send(new GetArticleQuery(slug), cancellationToken);

    if (result.IsSuccess)
    {
      Response = result.Value;
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


