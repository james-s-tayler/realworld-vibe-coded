using Server.Core.Interfaces;

namespace Server.UseCases.Articles.List;

public class ListArticlesHandler(IListArticlesQueryService _query)
  : IQueryHandler<ListArticlesQuery, Result<ArticlesEntitiesResult>>
{
  public async Task<Result<ArticlesEntitiesResult>> Handle(ListArticlesQuery request, CancellationToken cancellationToken)
  {
    var articles = await _query.ListAsync(
      request.Tag,
      request.Author,
      request.Favorited,
      request.Limit,
      request.Offset,
      request.CurrentUserId);

    var articlesCount = articles.Count();

    return Result.Success(new ArticlesEntitiesResult(articles.ToList(), articlesCount));
  }
}
