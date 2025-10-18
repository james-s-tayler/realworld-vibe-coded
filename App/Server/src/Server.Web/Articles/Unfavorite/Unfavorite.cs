using Server.Core.Interfaces;
using Server.UseCases.Articles;
using Server.UseCases.Articles.Unfavorite;
using Server.Web.Infrastructure;

namespace Server.Web.Articles.Unfavorite;

/// <summary>
/// Unfavorite article
/// </summary>
/// <remarks>
/// Remove article from favorites. Authentication required.
/// </remarks>
public class Unfavorite(IMediator _mediator, ICurrentUserService _currentUserService) : Endpoint<UnfavoriteArticleRequest, ArticleResponse, ArticleMapper>
{
  public override void Configure()
  {
    Delete("/api/articles/{slug}/favorite");
    AuthSchemes("Token");
    Summary(s =>
    {
      s.Summary = "Unfavorite article";
      s.Description = "Remove article from favorites. Authentication required.";
    });
  }

  public override async Task HandleAsync(UnfavoriteArticleRequest request, CancellationToken cancellationToken)
  {
    var userId = _currentUserService.GetRequiredCurrentUserId();

    var result = await _mediator.Send(new UnfavoriteArticleCommand(request.Slug, userId, userId), cancellationToken);

    await Send.ResultAsync(result, article => Map.FromEntity(article), cancellationToken);
  }
}
