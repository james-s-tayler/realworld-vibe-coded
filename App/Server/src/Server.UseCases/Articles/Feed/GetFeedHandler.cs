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

    // Map entities to DTOs in the Application layer
    var articleDtos = articles.Select(a => ArticleMappers.MapToDto(a, currentUser)).ToList();
    var articlesCount = articleDtos.Count;

    return Result.Success(new ArticlesResponse(articleDtos, articlesCount));
  }
}
