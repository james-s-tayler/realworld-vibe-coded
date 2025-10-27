using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Dtos;
using Server.Core.Interfaces;
using Server.Infrastructure;
using Server.UseCases.Articles;
using Server.UseCases.Articles.List;

namespace Server.Web.Articles.List;

/// <summary>
/// List articles
/// </summary>
/// <remarks>
/// List articles globally. Optional filters for tag, author, favorited user. Authentication optional.
/// </remarks>
public class ListArticles(IMediator _mediator, IUserContext userContext) : Endpoint<ListArticlesRequest, ArticlesResponse, ArticleMapper>
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
    var currentUserId = userContext.GetCurrentUserId();

    var result = await _mediator.Send(new ListArticlesQuery(
      request.Tag,
      request.Author,
      request.Favorited,
      request.Limit,
      request.Offset,
      currentUserId), cancellationToken);

    await Send.ResultMapperAsync(result, articles =>
    {
      var articleDtos = Enumerable.Select<Article, ArticleDto>(articles, article => Map.FromEntity(article).Article).ToList();
      return new ArticlesResponse(articleDtos, articleDtos.Count);
    }, cancellationToken);
  }
}
