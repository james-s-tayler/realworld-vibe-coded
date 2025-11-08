using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Specifications.Articles;
using Server.Core.UserAggregate;
using Server.Core.UserAggregate.Specifications;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Persistence;

namespace Server.UseCases.Articles.Feed;

public class GetFeedHandler(
  IReadRepository<User> userRepository,
  IReadRepository<Article> articleRepository)
  : IQueryHandler<GetFeedQuery, IEnumerable<Article>>
{
  public async Task<Result<IEnumerable<Article>>> Handle(GetFeedQuery request, CancellationToken cancellationToken)
  {
    // Get the user with their following relationships
    var user = await userRepository.FirstOrDefaultAsync(
      new UserWithFollowingSpec(request.UserId), cancellationToken);

    if (user == null)
    {
      return Result<IEnumerable<Article>>.Success(new List<Article>());
    }

    // Get IDs of users that the current user follows
    var followedUserIds = user.Following
      .Select(uf => uf.FollowedId)
      .ToList();

    // If not following anyone, return empty list
    if (!followedUserIds.Any())
    {
      return Result<IEnumerable<Article>>.Success(new List<Article>());
    }

    // Get articles from followed users using specification
    var spec = new FeedArticlesSpec(followedUserIds, request.Limit, request.Offset);
    var articles = await articleRepository.ListAsync(spec, cancellationToken);

    return Result<IEnumerable<Article>>.Success(articles);
  }
}
