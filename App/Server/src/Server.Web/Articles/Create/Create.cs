using Server.Infrastructure;
using Server.UseCases.Articles;
using Server.UseCases.Articles.Create;
using Server.UseCases.Interfaces;

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
    AuthSchemes(Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme, Microsoft.AspNetCore.Identity.IdentityConstants.BearerScheme);
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
      async (article, ct) => await Map.FromEntityAsync(article, ct),
      cancellationToken);
  }
}
