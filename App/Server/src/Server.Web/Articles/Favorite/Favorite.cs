using Server.Core.Interfaces;
using Server.UseCases.Articles;
using Server.UseCases.Articles.Favorite;
using Server.Web.Infrastructure;

namespace Server.Web.Articles.Favorite;

/// <summary>
/// Favorite article
/// </summary>
/// <remarks>
/// Add article to favorites. Authentication required.
/// </remarks>
public class Favorite(IMediator _mediator, ICurrentUserService _currentUserService) : Endpoint<FavoriteArticleRequest, ArticleResponse, ArticleMapper>
{
  public override void Configure()
  {
    Post("/api/articles/{slug}/favorite");
    AuthSchemes("Token");
    Summary(s =>
    {
      s.Summary = "Favorite article";
      s.Description = "Add article to favorites. Authentication required.";
    });
  }

  public override async Task HandleAsync(FavoriteArticleRequest request, CancellationToken cancellationToken)
  {
    var userId = _currentUserService.GetRequiredCurrentUserId();

    var result = await _mediator.Send(new FavoriteArticleCommand(request.Slug, userId, userId), cancellationToken);

    await Send.ResultAsync(result, article => Map.FromEntity(article), cancellationToken);
  }
}
