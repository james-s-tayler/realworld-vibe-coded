using Server.Infrastructure;
using Server.UseCases.Articles;
using Server.UseCases.Articles.Get;
using Server.UseCases.Interfaces;

namespace Server.Web.Articles.Get;

/// <summary>
/// Get article by slug
/// </summary>
/// <remarks>
/// Gets a single article by its slug. Authentication required.
/// </remarks>
public class Get(IMediator mediator, IUserContext userContext) : Endpoint<GetArticleRequest, ArticleResponse, ArticleMapper>
{
  public override void Configure()
  {
    Get("/api/articles/{slug}");
    AuthSchemes(Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme, Microsoft.AspNetCore.Identity.IdentityConstants.BearerScheme);
    Summary(s =>
    {
      s.Summary = "Get article by slug";
      s.Description = "Gets a single article by its slug. Authentication required.";
    });
  }

  public override async Task HandleAsync(GetArticleRequest request, CancellationToken cancellationToken)
  {
    // Get current user ID (authentication required)
    var currentUserId = userContext.GetRequiredCurrentUserId();

    var result = await mediator.Send(new GetArticleQuery(request.Slug, currentUserId), cancellationToken);

    await Send.ResultMapperAsync(result, async (article, ct) => await Map.FromEntityAsync(article, ct), cancellationToken);
  }
}
