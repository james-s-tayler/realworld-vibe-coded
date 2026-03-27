using Server.Infrastructure;
using Server.UseCases.Articles.Update;
using Server.UseCases.Interfaces;

namespace Server.Web.Articles.Update;

public class Update(IMediator mediator, IUserContext userContext) : Endpoint<UpdateArticleRequest, ArticleResponse, ArticleMapper>
{
  public override void Configure()
  {
    Put("/api/articles/{slug}");
    AuthSchemes(Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme, Microsoft.AspNetCore.Identity.IdentityConstants.BearerScheme);
  }

  public override async Task HandleAsync(UpdateArticleRequest request, CancellationToken cancellationToken)
  {
    var userId = userContext.GetRequiredCurrentUserId();

    var result = await mediator.Send(
      new UpdateArticleCommand(
        request.Slug,
        request.Article.Title,
        request.Article.Description,
        request.Article.Body,
        userId),
      cancellationToken);

    await Send.ResultMapperAsync(
      result,
      async (articleResult, ct) => await Map.FromEntityAsync(articleResult, ct),
      cancellationToken);
  }
}
