using Server.Core.ArticleAggregate;
using Server.Core.Interfaces;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Articles.Feed;

public class GetFeedHandler(IFeedQueryService _feedQuery)
  : IQueryHandler<GetFeedQuery, IEnumerable<Article>>
{
  public async Task<Result<IEnumerable<Article>>> Handle(GetFeedQuery request, CancellationToken cancellationToken)
  {
    var articles = await _feedQuery.GetFeedAsync(
      request.UserId,
      request.Limit,
      request.Offset);

    return Result<IEnumerable<Article>>.Success(articles);
  }
}
