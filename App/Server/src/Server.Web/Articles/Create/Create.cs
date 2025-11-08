using Server.Core.Interfaces;
using Server.Infrastructure;
using Server.UseCases.Articles;
using Server.UseCases.Articles.Create;

namespace Server.Web.Articles.Create;

/// <summary>
/// Create article
/// </summary>
/// <remarks>
/// Creates a new article. Authentication required.
/// </remarks>
public class Create(IMediator mediator, IUserContext userContext) : Endpoint<CreateArticleRequest, ArticleResponse, ArticleMapper>
{
  public override void Configure()
  {
    Post("/api/articles");
    AuthSchemes("Token");
    Summary(s =>
    {
      s.Summary = "Create article";
      s.Description = "Creates a new article. Authentication required.";
    });
  }

  public override async Task HandleAsync(CreateArticleRequest request, CancellationToken cancellationToken)
  {
    var userId = userContext.GetRequiredCurrentUserId();

    var result = await mediator.Send(
      new CreateArticleCommand(
        request.Article.Title,
        request.Article.Description,
        request.Article.Body,
        request.Article.TagList ?? new List<string>(),
        userId,
        userId),
      cancellationToken);

    await Send.ResultMapperAsync(
      result,
      article => Map.FromEntity(article),
      cancellationToken);
  }
}
