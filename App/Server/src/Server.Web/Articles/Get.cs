using Server.UseCases.Articles.Get;
using Server.UseCases.Articles;

namespace Server.Web.Articles;

/// <summary>
/// Get article by slug
/// </summary>
/// <remarks>
/// Gets a single article by its slug. Authentication optional.
/// </remarks>
public class Get(IMediator _mediator) : Endpoint<GetArticleRequest, ArticleResponse>
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
    var result = await _mediator.Send(new GetArticleQuery(request.Slug), cancellationToken);

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

public class GetArticleRequest
{
  public string Slug { get; set; } = string.Empty;
}