using Server.Infrastructure;
using Server.UseCases.Articles;
using Server.UseCases.Articles.Favorite;
using Server.UseCases.Interfaces;

namespace Server.Web.Articles.Favorite;

/// <summary>
/// Favorite article
/// </summary>
/// <remarks>
/// Add article to favorites. Authentication required.
/// </remarks>
public class Favorite(IMediator mediator, IUserContext userContext) : Endpoint<FavoriteArticleRequest, ArticleResponse, ArticleMapper>
{
  public override void Configure()
  {
    Post("/api/articles/{slug}/favorite");
    AuthSchemes("Token", "Identity.Application");
    Summary(s =>
    {
      s.Summary = "Favorite article";
      s.Description = "Add article to favorites. Authentication required.";
    });
  }

  public override async Task HandleAsync(FavoriteArticleRequest request, CancellationToken cancellationToken)
  {
    var userId = userContext.GetRequiredCurrentUserId();

    var result = await mediator.Send(new FavoriteArticleCommand(request.Slug, userId, userId), cancellationToken);

    await Send.ResultMapperAsync(result, async (article, ct) => await Map.FromEntityAsync(article, ct), cancellationToken);
  }
}
