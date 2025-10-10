using Server.Core.Interfaces;
using Server.Core.UserAggregate;
using Server.Core.UserAggregate.Specifications;

namespace Server.UseCases.Articles.Feed;

public class GetFeedHandler(
  IFeedQueryService _feedQuery,
  IRepository<User> _userRepository)
  : IQueryHandler<GetFeedQuery, Result<ArticlesResponse>>
{
  public async Task<Result<ArticlesResponse>> Handle(GetFeedQuery request, CancellationToken cancellationToken)
  {
    var articles = await _feedQuery.GetFeedAsync(
      request.UserId,
      request.Limit,
      request.Offset);

    // Get current user with following relationships
    var currentUser = await _userRepository.FirstOrDefaultAsync(
      new UserWithFollowingSpec(request.UserId), cancellationToken);

    // Use FastEndpoints-style mapper to convert entities to response
    var mapper = new ArticleResponseMapper(currentUser);
    var response = mapper.FromEntities(articles);

    return Result.Success(response);
  }
}
