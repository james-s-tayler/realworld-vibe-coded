using Server.UseCases.Articles;
using Server.UseCases.Articles.Get;

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
    DontThrowIfValidationFails();
    Summary(s =>
    {
      s.Summary = "Get article by slug";
      s.Description = "Gets a single article by its slug. Authentication optional.";
    });
  }

  public override async Task HandleAsync(GetArticleRequest request, CancellationToken cancellationToken)
  {
    // Check for validation errors manually
    if (ValidationFailed)
    {
      HttpContext.Response.StatusCode = 422;
      HttpContext.Response.ContentType = "application/json";

      var errors = new Dictionary<string, List<string>>();

      foreach (var failure in ValidationFailures)
      {
        var propertyName = failure.PropertyName.ToLowerInvariant();
        if (!errors.ContainsKey(propertyName))
        {
          errors[propertyName] = new List<string>();
        }
        errors[propertyName].Add(failure.ErrorMessage);
      }

      var validationErrorResponse = System.Text.Json.JsonSerializer.Serialize(new { errors });
      await HttpContext.Response.WriteAsync(validationErrorResponse, cancellationToken);
      return;
    }

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
