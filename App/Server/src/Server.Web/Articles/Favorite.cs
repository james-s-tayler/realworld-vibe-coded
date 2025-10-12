using Server.Core.Interfaces;
using Server.UseCases.Articles;
using Server.UseCases.Articles.Favorite;
using Server.Web.Infrastructure;

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
      await HttpContext.Response.HttpContext.Response.SendAsync(new ConduitErrorResponse
      {
        Errors = new ConduitErrorBody { Body = new[] { "Article not found" } }
      }, 404);
      return;
    }

    await HttpContext.Response.HttpContext.Response.SendAsync(new ConduitErrorResponse
    {
      Errors = new ConduitErrorBody { Body = result.Errors.ToArray() }
    }, 400);
  }
}
