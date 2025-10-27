using Server.Core.ArticleAggregate;
using Server.Core.Interfaces;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Articles.List;

public class ListArticlesHandler(IListArticlesQueryService _query)
  : IQueryHandler<ListArticlesQuery, IEnumerable<Article>>
{
  public async Task<Result<IEnumerable<Article>>> Handle(ListArticlesQuery request, CancellationToken cancellationToken)
  {
    var articles = await _query.ListAsync(
      request.Tag,
      request.Author,
      request.Favorited,
      request.Limit,
      request.Offset,
      request.CurrentUserId);

    return Result.Success(articles);
  }
}
