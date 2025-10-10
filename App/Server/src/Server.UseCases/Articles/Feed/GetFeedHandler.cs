using Server.Core.Interfaces;

namespace Server.UseCases.Articles.Feed;

public class GetFeedHandler(IFeedQueryService _feedQuery)
  : IQueryHandler<GetFeedQuery, Result<ArticlesResult>>
{
  public async Task<Result<ArticlesResult>> Handle(GetFeedQuery request, CancellationToken cancellationToken)
  {
    var articles = await _feedQuery.GetFeedAsync(
      request.UserId,
      request.Limit,
      request.Offset);

    var articlesCount = articles.Count();

    // Return entities - mapping will happen in the endpoint
    return Result.Success(new ArticlesResult(articles.ToList(), articlesCount));
  }
}
