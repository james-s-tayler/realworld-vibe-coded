using Server.Core.Interfaces;
using Server.UseCases.Articles;
using Server.UseCases.Articles.Update;
using Server.Web.Infrastructure;

namespace Server.Web.Articles;

/// <summary>
/// Update article
/// </summary>
/// <remarks>
/// Updates an existing article. Authentication required. User must be the author.
/// </remarks>
public class Update(IMediator _mediator, ICurrentUserService _currentUserService) : Endpoint<UpdateArticleRequest, ArticleResponse, ArticleMapper>
{
  public override void Configure()
  {
    Put("/api/articles/{slug}");
    AuthSchemes("Token");
    Summary(s =>
    {
      s.Summary = "Update article";
      s.Description = "Updates an existing article. Authentication required. User must be the author.";
    });
  }

  public override void OnValidationFailed()
  {
    var errorBody = new List<string>();

    foreach (var failure in ValidationFailures)
    {
      // Handle nested properties like Article.Title -> title
      var propertyName = failure.PropertyName.ToLower();
      if (propertyName.Contains('.'))
      {
        propertyName = propertyName.Split('.').Last();
      }

      // Handle array indexing for tags like Article.TagList[0] -> taglist[0]
      if (propertyName.Contains("taglist["))
      {
        // Already in the right format, just ensure lowercase
        propertyName = propertyName.Replace("taglist", "taglist");
      }

      errorBody.Add($"{propertyName} {failure.ErrorMessage}");
    }

    HttpContext.Response.SendAsync(new ConduitErrorResponse
    {
      Errors = new ConduitErrorBody { Body = errorBody.ToArray() }
    }, 422).GetAwaiter().GetResult();
  }

  public override async Task HandleAsync(UpdateArticleRequest request, CancellationToken cancellationToken)
  {
    var userId = _currentUserService.GetRequiredCurrentUserId();

    var result = await _mediator.Send(new UpdateArticleCommand(
      request.Slug,
      request.Article.Title,
      request.Article.Description,
      request.Article.Body,
      userId,
      userId), cancellationToken);

    if (result.IsSuccess)
    {
      // Use FastEndpoints mapper to convert entity to response DTO
      Response = Map.FromEntity(result.Value);
      return;
    }

    if (result.Status == ResultStatus.NotFound)
    {
      await HttpContext.Response.HttpContext.Response.SendAsync(new ConduitErrorResponse
      {
        Errors = new ConduitErrorBody { Body = new[] { "Article not found" } }
      }, 404);
      return;
    }

    if (result.Status == ResultStatus.Forbidden)
    {
      await HttpContext.Response.HttpContext.Response.SendAsync(new ConduitErrorResponse
      {
        Errors = new ConduitErrorBody { Body = new[] { "You can only update your own articles" } }
      }, 403);
      return;
    }

    await HttpContext.Response.HttpContext.Response.SendAsync(new ConduitErrorResponse
    {
      Errors = new ConduitErrorBody { Body = result.Errors.ToArray() }
    }, 422);
  }
}

public class UpdateArticleRequest
{
  public string Slug { get; set; } = string.Empty;
  public UpdateArticleData Article { get; set; } = new();
}

public class UpdateArticleData
{
  public string? Title { get; set; } = null;
  public string? Description { get; set; } = null;
  public string? Body { get; set; } = null;
}
