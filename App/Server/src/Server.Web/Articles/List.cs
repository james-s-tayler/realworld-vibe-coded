using Server.Core.Interfaces;
using Server.UseCases.Articles;
using Server.UseCases.Articles.List;
using Server.Web.Infrastructure;

namespace Server.Web.Articles;

/// <summary>
/// List articles
/// </summary>
/// <remarks>
/// List articles globally. Optional filters for tag, author, favorited user. Authentication optional.
/// </remarks>
public class List(IMediator _mediator, ICurrentUserService _currentUserService) : EndpointWithoutRequest<ArticlesResponse, ArticleMapper>
{
  public override void Configure()
  {
    Get("/api/articles");
    AllowAnonymous();
    Summary(s =>
    {
      s.Summary = "List articles";
      s.Description = "List articles globally. Optional filters for tag, author, favorited user. Authentication optional.";
    });
  }

  public override async Task HandleAsync(CancellationToken cancellationToken)
  {
    var validation = QueryParameterValidator.ValidateListArticlesParameters(HttpContext.Request);

    if (!validation.IsValid)
    {
      await this.SendValidationErrorAsync(validation.Errors, cancellationToken);
      return;
    }

    // Get current user ID if authenticated
    var currentUserId = _currentUserService.GetCurrentUserId();

    var result = await _mediator.Send(new ListArticlesQuery(
      validation.Tag,
      validation.Author,
      validation.Favorited,
      validation.Limit,
      validation.Offset,
      currentUserId), cancellationToken);

    await this.SendAsync(result, articles =>
    {
      var articleDtos = articles.Select(article => Map.FromEntity(article).Article).ToList();
      return new ArticlesResponse(articleDtos, articleDtos.Count);
    }, cancellationToken);
  }
}
