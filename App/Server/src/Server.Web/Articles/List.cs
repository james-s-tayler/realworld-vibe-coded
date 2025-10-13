using Server.Core.Interfaces;
using Server.UseCases.Articles;
using Server.UseCases.Articles.List;

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
      await SendAsync(new
      {
        errors = new { body = validation.Errors.ToArray() }
      }, 422, cancellationToken);
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

    if (result.IsSuccess)
    {
      // Map each Article entity to ArticleDto using FastEndpoints mapper
      var articles = result.Value.ToList();
      var articleDtos = articles.Select(article => Map.FromEntity(article).Article).ToList();
      Response = new ArticlesResponse(articleDtos, articleDtos.Count);
      return;
    }

    await SendAsync(new
    {
      errors = new { body = new[] { result.Errors.FirstOrDefault() ?? "Failed to retrieve articles" } }
    }, 400, cancellationToken);
  }
}
