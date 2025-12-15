using Server.Infrastructure;
using Server.UseCases.Articles;
using Server.UseCases.Articles.Get;
using Server.UseCases.Interfaces;

namespace Server.Web.Articles.Get;

/// <summary>
/// Get article by slug
/// </summary>
/// <remarks>
/// Gets a single article by its slug. Authentication optional.
/// </remarks>
public class Get(IMediator mediator, IUserContext userContext) : Endpoint<GetArticleRequest, ArticleResponse, ArticleMapper>
{
  public override void Configure()
  {
    Get("/api/articles/{slug}");
    AllowAnonymous();
    AuthSchemes("Token", Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
    Summary(s =>
    {
      s.Summary = "Get article by slug";
      s.Description = "Gets a single article by its slug. Authentication optional.";
    });
  }

  public override async Task HandleAsync(GetArticleRequest request, CancellationToken cancellationToken)
  {
    // Get current user ID if authenticated
    var currentUserId = userContext.GetCurrentUserId();

    var result = await mediator.Send(new GetArticleQuery(request.Slug, currentUserId), cancellationToken);

    await Send.ResultMapperAsync(result, async (article, ct) => await Map.FromEntityAsync(article, ct), cancellationToken);
  }
}
