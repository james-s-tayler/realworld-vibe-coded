using Server.Core.Interfaces;

namespace Server.UseCases.Articles.List;

public class ListArticlesHandler(IListArticlesQueryService _query)
  : IQueryHandler<ListArticlesQuery, Result<ArticlesResult>>
{
  public async Task<Result<ArticlesResult>> Handle(ListArticlesQuery request, CancellationToken cancellationToken)
  {
    var articles = await _query.ListAsync(
      request.Tag,
      request.Author,
      request.Favorited,
      request.Limit,
      request.Offset);

    var articlesCount = articles.Count();

    // Return entities - mapping will happen in the endpoint
    return Result.Success(new ArticlesResult(articles.ToList(), articlesCount));
  }
}
