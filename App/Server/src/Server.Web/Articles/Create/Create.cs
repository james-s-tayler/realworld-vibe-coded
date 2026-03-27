using Server.Infrastructure;
using Server.UseCases.Articles.Create;
using Server.UseCases.Interfaces;

namespace Server.Web.Articles.Create;

public class Create(IMediator mediator, IUserContext userContext) : Endpoint<CreateArticleRequest, ArticleResponse, ArticleMapper>
{
  public override void Configure()
  {
    Post("/api/articles");
    AuthSchemes(Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme, Microsoft.AspNetCore.Identity.IdentityConstants.BearerScheme);
  }

  public override async Task HandleAsync(CreateArticleRequest request, CancellationToken cancellationToken)
  {
    var userId = userContext.GetRequiredCurrentUserId();

    var result = await mediator.Send(
      new CreateArticleCommand(
        request.Article.Title,
        request.Article.Description,
        request.Article.Body,
        request.Article.TagList ?? [],
        userId),
      cancellationToken);

    await Send.ResultMapperAsync(
      result,
      async (articleResult, ct) => await Map.FromEntityAsync(articleResult, ct),
      cancellationToken);
  }
}
