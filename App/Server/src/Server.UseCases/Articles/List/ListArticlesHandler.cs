using Server.Core.Interfaces;

namespace Server.UseCases.Articles.List;

public class ListArticlesHandler(IListArticlesQueryService _query)
  : IQueryHandler<ListArticlesQuery, Result<ArticlesResponse>>
{
  public async Task<Result<ArticlesResponse>> Handle(ListArticlesQuery request, CancellationToken cancellationToken)
  {
    var articles = await _query.ListAsync(
      request.Tag,
      request.Author,
      request.Favorited,
      request.Limit,
      request.Offset,
      request.CurrentUserId);

    var articlesCount = articles.Count();

    return Result.Success(new ArticlesResponse(articles.ToList(), articlesCount));
  }
}
