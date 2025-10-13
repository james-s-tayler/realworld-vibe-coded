using Server.Core.Interfaces;
using Server.UseCases.Articles;
using Server.UseCases.Articles.Create;

namespace Server.Web.Articles;

/// <summary>
/// Create article
/// </summary>
/// <remarks>
/// Creates a new article. Authentication required.
/// </remarks>
public class Create(IMediator _mediator, ICurrentUserService _currentUserService) : Endpoint<CreateArticleRequest, ArticleResponse, ArticleMapper>
{
  public override void Configure()
  {
    Post("/api/articles");
    AuthSchemes("Token");
    DontAutoTag();
    Summary(s =>
    {
      s.Summary = "Create article";
      s.Description = "Creates a new article. Authentication required.";
    });
  }

  public override async Task HandleAsync(CreateArticleRequest request, CancellationToken cancellationToken)
  {
    var userId = _currentUserService.GetRequiredCurrentUserId();

    var result = await _mediator.Send(new CreateArticleCommand(
      request.Article.Title,
      request.Article.Description,
      request.Article.Body,
      request.Article.TagList ?? new List<string>(),
      userId,
      userId), cancellationToken);

    if (result.IsSuccess)
    {
      // Use FastEndpoints mapper to convert entity to response DTO
      await SendAsync(Map.FromEntity(result.Value), 201, cancellationToken);
      return;
    }

    await SendAsync(new
    {
      errors = new { body = result.Errors.ToArray() }
    }, 422, cancellationToken);
  }
}
