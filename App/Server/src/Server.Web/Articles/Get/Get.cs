using Server.Core.Interfaces;
using Server.UseCases.Articles;
using Server.UseCases.Articles.Get;
using Server.Web.Infrastructure;

namespace Server.Web.Articles.Get;

/// <summary>
/// Get article by slug
/// </summary>
/// <remarks>
/// Gets a single article by its slug. Authentication optional.
/// </remarks>
public class Get(IMediator _mediator, ICurrentUserService _currentUserService) : Endpoint<GetArticleRequest, ArticleResponse, ArticleMapper>
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

  public override async Task HandleAsync(GetArticleRequest request, CancellationToken cancellationToken)
  {
    // Get current user ID if authenticated
    var currentUserId = _currentUserService.GetCurrentUserId();

    var result = await _mediator.Send(new GetArticleQuery(request.Slug, currentUserId), cancellationToken);

    await Send.ResultAsync(result, article => Map.FromEntity(article), cancellationToken);
  }
}


