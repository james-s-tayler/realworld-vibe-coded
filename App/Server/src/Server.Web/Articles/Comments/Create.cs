using System.Security.Claims;
using FastEndpoints;
using MediatR;
using Server.Core.ArticleAggregate.Dtos;
using Server.UseCases.Articles.Comments.Create;

namespace Server.Web.Articles.Comments;

/// <summary>
/// Create comment for an article
/// </summary>
/// <remarks>
/// Create a new comment for an article. Authentication required.
/// </remarks>
public class Create(IMediator _mediator) : Endpoint<CreateCommentRequest, CommentResponse>
{
  public override void Configure()
  {
    Post("/api/articles/{slug}/comments");
    AuthSchemes("Token");
    Summary(s =>
    {
      s.Summary = "Create comment";
      s.Description = "Create a new comment for an article. Authentication required.";
    });
  }

  public override void OnValidationFailed()
  {
    var errorBody = new List<string>();

    foreach (var failure in ValidationFailures)
    {
      // Handle nested properties like Comment.Body -> body
      var propertyName = failure.PropertyName.ToLower();
      if (propertyName.Contains('.'))
      {
        propertyName = propertyName.Split('.').Last();
      }

      errorBody.Add($"{propertyName} {failure.ErrorMessage}");
    }

    HttpContext.Response.StatusCode = 422;
    HttpContext.Response.ContentType = "application/json";
    var json = System.Text.Json.JsonSerializer.Serialize(new
    {
      errors = new { body = errorBody }
    });
    HttpContext.Response.WriteAsync(json).GetAwaiter().GetResult();
  }

  public override async Task HandleAsync(CreateCommentRequest request, CancellationToken cancellationToken)
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

    // Get slug from route parameter
    var slug = Route<string>("slug") ?? string.Empty;

    var result = await _mediator.Send(new CreateCommentCommand(slug, request.Comment.Body, userId, userId), cancellationToken);

    if (result.IsSuccess)
    {
      HttpContext.Response.StatusCode = 201;
      Response = result.Value;
      return;
    }

    if (result.Status == Ardalis.Result.ResultStatus.NotFound)
    {
      HttpContext.Response.StatusCode = 422;
      HttpContext.Response.ContentType = "application/json";
      var notFoundJson = System.Text.Json.JsonSerializer.Serialize(new
      {
        errors = new { body = new[] { "Article not found" } }
      });
      await HttpContext.Response.WriteAsync(notFoundJson, cancellationToken);
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

public class CreateCommentRequest
{
  public CreateCommentDto Comment { get; set; } = default!;
}

public class CreateCommentDto
{
  public string Body { get; set; } = string.Empty;
}
