using System.Security.Claims;
using Server.UseCases.Articles;
using Server.UseCases.Articles.Create;

namespace Server.Web.Articles;

/// <summary>
/// Create article
/// </summary>
/// <remarks>
/// Creates a new article. Authentication required.
/// </remarks>
public class Create(IMediator _mediator) : Endpoint<CreateArticleRequest, ArticleResponse>
{
  public override void Configure()
  {
    Post("/api/articles");
    AuthSchemes("Token");
    DontAutoTag();
    DontThrowIfValidationFails();
    Summary(s =>
    {
      s.Summary = "Create article";
      s.Description = "Creates a new article. Authentication required.";
    });
  }

  public override async Task HandleAsync(CreateArticleRequest request, CancellationToken cancellationToken)
  {
    // Check for validation errors manually
    if (ValidationFailed)
    {
      HttpContext.Response.StatusCode = 422;
      HttpContext.Response.ContentType = "application/json";

      var errors = new Dictionary<string, List<string>>();

      foreach (var failure in ValidationFailures)
      {
        var originalPropertyName = failure.PropertyName;
        var propertyName = failure.PropertyName.ToLowerInvariant();

        // Handle nested properties like Article.Title -> title
        if (propertyName.Contains('.'))
        {
          propertyName = propertyName.Split('.').Last();
        }

        // Handle array indexing for tags like Article.TagList[0] -> tagList[0]
        if (originalPropertyName.Contains("TagList["))
        {
          // Extract the index and format as tagList[index]
          var indexMatch = System.Text.RegularExpressions.Regex.Match(originalPropertyName, @"TagList\[(\d+)\]");
          if (indexMatch.Success)
          {
            propertyName = $"taglist[{indexMatch.Groups[1].Value}]";
          }
        }

        if (!errors.ContainsKey(propertyName))
        {
          errors[propertyName] = new List<string>();
        }

        // Use the error message as-is since we've already formatted it in the validator
        errors[propertyName].Add(failure.ErrorMessage);
      }

      var validationErrorResponse = System.Text.Json.JsonSerializer.Serialize(new { errors });
      await HttpContext.Response.WriteAsync(validationErrorResponse, cancellationToken);
      return;
    }

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

    var result = await _mediator.Send(new CreateArticleCommand(
      request.Article.Title,
      request.Article.Description,
      request.Article.Body,
      request.Article.TagList ?? new List<string>(),
      userId), cancellationToken);

    if (result.IsSuccess)
    {
      HttpContext.Response.StatusCode = 201;
      Response = result.Value;
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
