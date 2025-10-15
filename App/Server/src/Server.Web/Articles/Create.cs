using Server.Core.Interfaces;
using Server.UseCases.Articles;
using Server.UseCases.Articles.Create;
using Server.Web.Infrastructure;

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

    await Send.ResultAsync(result, article => Map.FromEntity(article), cancellationToken);
  }
}
