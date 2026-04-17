using Server.Core.ArticleAggregate.Dtos;
using Server.Infrastructure;
using Server.SharedKernel.Pagination;
using Server.UseCases.Articles.List;
using Server.UseCases.Interfaces;

namespace Server.Web.Articles.List;

/// <summary>
/// List articles
/// </summary>
/// <remarks>
/// List articles globally. Optional filters for tag, author, favorited user. Authentication required.
/// </remarks>
public class ListArticles(IMediator mediator, IUserContext userContext)
  : Endpoint<ListArticlesRequest, PaginatedResponse<ArticleDto>, ListArticlesMapper>
{
  public override void Configure()
  {
    Get("/api/articles");
    AuthSchemes(Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme, Microsoft.AspNetCore.Identity.IdentityConstants.BearerScheme);
    Summary(s =>
    {
      s.Summary = "List articles";
      s.Description = "List articles globally. Optional filters for tag, author, favorited user. Authentication required.";
    });
  }

  public override async Task HandleAsync(ListArticlesRequest request, CancellationToken cancellationToken)
  {
    var currentUserId = userContext.GetRequiredCurrentUserId();

    var result = await mediator.Send(
      new ListArticlesQuery(
        request.Tag,
        request.Author,
        request.Favorited,
        request.Limit,
        request.Offset,
        currentUserId),
      cancellationToken);

    await Send.ResultMapperAsync(result, Map.FromEntityAsync, cancellationToken);
  }
}
