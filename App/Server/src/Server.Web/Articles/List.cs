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
public class List(IMediator _mediator, ICurrentUserService _currentUserService) : Endpoint<ListArticlesRequest, ArticlesResponse, ArticleMapper>
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

  public override async Task HandleAsync(ListArticlesRequest request, CancellationToken cancellationToken)
  {
    // Get current user ID if authenticated
    var currentUserId = _currentUserService.GetCurrentUserId();

    // Parse query parameters with defaults
    var limit = string.IsNullOrEmpty(request.Limit) ? 20 : int.Parse(request.Limit);
    var offset = string.IsNullOrEmpty(request.Offset) ? 0 : int.Parse(request.Offset);

    var result = await _mediator.Send(new ListArticlesQuery(
      request.Tag,
      request.Author,
      request.Favorited,
      limit,
      offset,
      currentUserId), cancellationToken);

    await this.SendAsync(result, articles =>
    {
      var articleDtos = articles.Select(article => Map.FromEntity(article).Article).ToList();
      return new ArticlesResponse(articleDtos, articleDtos.Count);
    }, cancellationToken);
  }
}

public class ListArticlesRequest
{
  public string? Tag { get; set; }
  public string? Author { get; set; }
  public string? Favorited { get; set; }
  public string? Limit { get; set; }
  public string? Offset { get; set; }
}
