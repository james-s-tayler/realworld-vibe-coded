using Server.Infrastructure;
using Server.UseCases.Articles;
using Server.UseCases.Articles.Unfavorite;
using Server.UseCases.Interfaces;

namespace Server.Web.Articles.Unfavorite;

/// <summary>
/// Unfavorite article
/// </summary>
/// <remarks>
/// Remove article from favorites. Authentication required.
/// </remarks>
public class Unfavorite(IMediator mediator, IUserContext userContext) : Endpoint<UnfavoriteArticleRequest, ArticleResponse, ArticleMapper>
{
  public override void Configure()
  {
    Delete("/api/articles/{slug}/favorite");
    AuthSchemes("Token", "Identity.Application");
    Summary(s =>
    {
      s.Summary = "Unfavorite article";
      s.Description = "Remove article from favorites. Authentication required.";
    });
  }

  public override async Task HandleAsync(UnfavoriteArticleRequest request, CancellationToken cancellationToken)
  {
    var userId = userContext.GetRequiredCurrentUserId();

    var result = await mediator.Send(new UnfavoriteArticleCommand(request.Slug, userId, userId), cancellationToken);

    await Send.ResultMapperAsync(result, async (article, ct) => await Map.FromEntityAsync(article, ct), cancellationToken);
  }
}
