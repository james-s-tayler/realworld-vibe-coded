using Server.Infrastructure;
using Server.UseCases.Articles;
using Server.UseCases.Articles.List;
using Server.UseCases.Interfaces;

namespace Server.Web.Articles.List;

/// <summary>
/// List articles
/// </summary>
/// <remarks>
/// List articles globally. Optional filters for tag, author, favorited user. Authentication optional.
/// </remarks>
public class ListArticles(IMediator mediator, IUserContext userContext) : Endpoint<ListArticlesRequest, ArticlesResponse, ArticleMapper>
{
  public override void Configure()
  {
    Get("/api/articles");
    AuthSchemes("Token", Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme);
    Options(x => x.AllowAnonymous());
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

    var result = await mediator.Send(
      new ListArticlesQuery(
        request.Tag,
        request.Author,
        request.Favorited,
        request.Limit,
        request.Offset,
        currentUserId),
      cancellationToken);

    await Send.ResultMapperAsync(
      result,
      async (listResult, ct) =>
      {
        var articleDtos = new List<Server.Core.ArticleAggregate.Dtos.ArticleDto>();
        foreach (var article in listResult.Articles)
        {
          var response = await Map.FromEntityAsync(article, ct);
          articleDtos.Add(response.Article);
        }

        return new ArticlesResponse(articleDtos, listResult.TotalCount);
      },
      cancellationToken);
  }
}
