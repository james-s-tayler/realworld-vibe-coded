using System.Security.Claims;
using Server.Core.ArticleAggregate.Dtos;
using Server.UseCases.Articles.Create;

namespace Server.Web.Articles;

/// <summary>
/// Create article
/// </summary>
/// <remarks>
/// Create a new article. Authentication required.
/// </remarks>
public class CreateArticle(IMediator _mediator) : Endpoint<CreateArticleRequest, CreateArticleResponse>
{
  public override void Configure()
  {
    Post(CreateArticleRequest.Route);
    AuthSchemes("Token");
    Summary(s =>
    {
      s.Summary = "Create article";
      s.Description = "Create a new article. Authentication required.";
      s.ExampleRequest = new CreateArticleRequest
      {
        Article = new ArticleData
        {
          Title = "How to train your dragon",
          Description = "Ever wonder how?",
          Body = "Very carefully.",
          TagList = new List<string> { "training", "dragons" }
        }
      };
    });
  }

  public override void OnValidationFailed()
  {
    var errorBody = new List<string>();

    foreach (var failure in ValidationFailures)
    {
      errorBody.Add($"{failure.PropertyName} {failure.ErrorMessage}");
    }

    HttpContext.Response.StatusCode = 422;
    HttpContext.Response.ContentType = "application/json";
    var json = System.Text.Json.JsonSerializer.Serialize(new
    {
      errors = new { body = errorBody }
    });
    HttpContext.Response.WriteAsync(json).GetAwaiter().GetResult();
  }

  public override async Task HandleAsync(
    CreateArticleRequest request,
    CancellationToken cancellationToken)
  {
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

    var result = await _mediator.Send(new CreateArticleCommand(
      userId,
      request.Article.Title,
      request.Article.Description,
      request.Article.Body,
      request.Article.TagList), cancellationToken);

    if (result.IsSuccess)
    {
      Response = new CreateArticleResponse { Article = result.Value };
      HttpContext.Response.StatusCode = 201;
      return;
    }

    if (result.IsInvalid())
    {
      var errorBody = new List<string>();
      foreach (var error in result.ValidationErrors)
      {
        errorBody.Add($"{error.Identifier} {error.ErrorMessage}");
      }

      HttpContext.Response.StatusCode = 422;
      HttpContext.Response.ContentType = "application/json";
      var json = System.Text.Json.JsonSerializer.Serialize(new
      {
        errors = new { body = errorBody }
      });
      await HttpContext.Response.WriteAsync(json, cancellationToken);
      return;
    }

    HttpContext.Response.StatusCode = 400;
    HttpContext.Response.ContentType = "application/json";
    var errorJson = System.Text.Json.JsonSerializer.Serialize(new
    {
      errors = new { body = new[] { result.Errors.FirstOrDefault() ?? "Article creation failed" } }
    });
    await HttpContext.Response.WriteAsync(errorJson, cancellationToken);
  }
}

public class CreateArticleResponse
{
  public ArticleDto Article { get; set; } = default!;
}
