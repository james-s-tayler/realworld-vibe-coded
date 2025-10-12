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

    if (result.IsSuccess)
    {
      // Use FastEndpoints mapper to convert entity to response DTO
      Response = Map.FromEntity(result.Value);
      return;
    }

    await HttpContext.Response.HttpContext.Response.SendAsync(new ConduitErrorResponse
    {
      Errors = new ConduitErrorBody { Body = new[] { "Article not found" } }
    }, 404);
  }
}


