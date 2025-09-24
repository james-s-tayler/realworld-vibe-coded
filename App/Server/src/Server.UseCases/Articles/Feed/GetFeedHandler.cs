using Server.Core.Interfaces;

namespace Server.UseCases.Articles.Feed;

public class GetFeedHandler(IFeedQueryService _feedQuery)
  : IQueryHandler<GetFeedQuery, Result<ArticlesResponse>>
{
  public async Task<Result<ArticlesResponse>> Handle(GetFeedQuery request, CancellationToken cancellationToken)
  {
    var articles = await _feedQuery.GetFeedAsync(
      request.UserId,
      request.Limit,
      request.Offset);

    var articlesCount = articles.Count();

    return Result.Success(new ArticlesResponse(articles.ToList(), articlesCount));
  }
}