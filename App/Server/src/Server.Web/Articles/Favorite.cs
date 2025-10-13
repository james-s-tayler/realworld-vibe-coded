using Server.Core.Interfaces;
using Server.UseCases.Articles;
using Server.UseCases.Articles.Favorite;

namespace Server.Web.Articles;

/// <summary>
/// Favorite article
/// </summary>
/// <remarks>
/// Add article to favorites. Authentication required.
/// </remarks>
public class Favorite(IMediator _mediator, ICurrentUserService _currentUserService) : EndpointWithoutRequest<ArticleResponse, ArticleMapper>
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

  public override async Task HandleAsync(CancellationToken cancellationToken)
  {
    // Get slug from route parameter
    var slug = Route<string>("slug") ?? string.Empty;

    var userId = _currentUserService.GetRequiredCurrentUserId();

    var result = await _mediator.Send(new FavoriteArticleCommand(slug, userId, userId), cancellationToken);

    if (result.IsSuccess)
    {
      // Use FastEndpoints mapper to convert entity to response DTO
      Response = Map.FromEntity(result.Value);
      return;
    }

    if (result.Status == ResultStatus.NotFound)
    {
      await SendAsync(new
      {
        errors = new { body = new[] { "Article not found" } }
      }, 404, cancellationToken);
      return;
    }

    await SendAsync(new
    {
      errors = new { body = result.Errors.ToArray() }
    }, 400, cancellationToken);
  }
}
