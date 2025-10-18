using Server.Core.Interfaces;
using Server.UseCases.Articles;
using Server.UseCases.Articles.Unfavorite;
using Server.Web.Infrastructure;

namespace Server.Web.Articles;

/// <summary>
/// Unfavorite article
/// </summary>
/// <remarks>
/// Remove article from favorites. Authentication required.
/// </remarks>
public class Unfavorite(IMediator _mediator, ICurrentUserService _currentUserService) : EndpointWithoutRequest<ArticleResponse, ArticleMapper>
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

  public override async Task HandleAsync(CancellationToken cancellationToken)
  {
    // Get slug from route parameter
    var slug = Route<string>("slug") ?? string.Empty;

    var userId = _currentUserService.GetRequiredCurrentUserId();

    var result = await _mediator.Send(new UnfavoriteArticleCommand(slug, userId, userId), cancellationToken);

    await Send.ResultAsync(result, article => Map.FromEntity(article), cancellationToken);
  }
}
