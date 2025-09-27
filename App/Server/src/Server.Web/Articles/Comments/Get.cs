using Server.Core.ArticleAggregate.Dtos;
using Server.Core.Interfaces;
using Server.UseCases.Articles.Comments.Get;

namespace Server.Web.Articles.Comments;

/// <summary>
/// Get article comments
/// </summary>
/// <remarks>
/// Get all comments for an article. Authentication optional.
/// </remarks>
public class Get(IMediator _mediator, ICurrentUserService _currentUserService) : Endpoint<GetCommentsRequest, CommentsResponse>
{
  public override void Configure()
  {
    Get("/api/articles/{slug}/comments");
    AllowAnonymous();
    Summary(s =>
    {
      s.Summary = "Get article comments";
      s.Description = "Get all comments for an article. Authentication optional.";
    });
  }

  public override async Task HandleAsync(GetCommentsRequest request, CancellationToken cancellationToken)
  {
    // Get current user ID if authenticated
    var currentUserId = _currentUserService.GetCurrentUserId();

    var result = await _mediator.Send(new GetCommentsQuery(request.Slug, currentUserId), cancellationToken);

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

    HttpContext.Response.StatusCode = 400;
    HttpContext.Response.ContentType = "application/json";
    var errorResponse = System.Text.Json.JsonSerializer.Serialize(new
    {
      errors = new { body = result.Errors.ToArray() }
    });
    await HttpContext.Response.WriteAsync(errorResponse, cancellationToken);
  }
}

public class GetCommentsRequest
{
  public string Slug { get; set; } = string.Empty;
}
