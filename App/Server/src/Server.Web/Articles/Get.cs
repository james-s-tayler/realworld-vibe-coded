using Server.Core.Interfaces;
using Server.UseCases.Articles;
using Server.UseCases.Articles.Get;
using Server.Web.Infrastructure;

namespace Server.Web.Articles;

/// <summary>
/// Get article by slug
/// </summary>
/// <remarks>
/// Gets a single article by its slug. Authentication optional.
/// </remarks>
public class Get(IMediator _mediator, ICurrentUserService _currentUserService) : EndpointWithoutRequest<ArticleResponse, ArticleMapper>
{
  public override void Configure()
  {
    Get("/api/articles/{slug}");
    AllowAnonymous();
    Summary(s =>
    {
      s.Summary = "Get article by slug";
      s.Description = "Gets a single article by its slug. Authentication optional.";
    });
  }

  public override async Task HandleAsync(CancellationToken cancellationToken)
  {
    // Get slug from route parameter
    var slug = Route<string>("slug") ?? string.Empty;

    // Get current user ID if authenticated
    var currentUserId = _currentUserService.GetCurrentUserId();

    var result = await _mediator.Send(new GetArticleQuery(slug, currentUserId), cancellationToken);

    await this.SendAsync(result, article => Map.FromEntity(article), cancellationToken);
  }
}


