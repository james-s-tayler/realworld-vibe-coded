using Server.Core.Interfaces;
using Server.UseCases.Articles;
using Server.UseCases.Articles.Update;

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
      await SendAsync(new
      {
        errors = new { body = new[] { "Article not found" } }
      }, 404, cancellationToken);
      return;
    }

    if (result.Status == ResultStatus.Forbidden)
    {
      await SendAsync(new
      {
        errors = new { body = new[] { "You can only update your own articles" } }
      }, 403, cancellationToken);
      return;
    }

    await SendAsync(new
    {
      errors = new { body = result.Errors.ToArray() }
    }, 422, cancellationToken);
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
