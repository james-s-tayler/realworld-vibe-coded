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
  : IQueryHandler<GetFeedQuery, (IEnumerable<Article> Articles, int TotalCount)>
{
  public async Task<Result<(IEnumerable<Article> Articles, int TotalCount)>> Handle(GetFeedQuery request, CancellationToken cancellationToken)
  {
    // Get the user with their following relationships
    var user = await userRepository.FirstOrDefaultAsync(
      new UserWithFollowingSpec(request.UserId), cancellationToken);

    if (user == null)
    {
      return Result<(IEnumerable<Article> Articles, int TotalCount)>.Success((new List<Article>(), 0));
    }

    // Get IDs of users that the current user follows
    var followedUserIds = user.Following
      .Select(uf => uf.FollowedId)
      .ToList();

    // If not following anyone, return empty list
    if (!followedUserIds.Any())
    {
      return Result<(IEnumerable<Article> Articles, int TotalCount)>.Success((new List<Article>(), 0));
    }

    // Get articles from followed users using specification
    var spec = new FeedArticlesSpec(followedUserIds, request.Limit, request.Offset);
    var countSpec = new FeedArticlesCountSpec(followedUserIds);

    var articles = await articleRepository.ListAsync(spec, cancellationToken);
    var totalCount = await articleRepository.CountAsync(countSpec, cancellationToken);

    return Result<(IEnumerable<Article> Articles, int TotalCount)>.Success((articles, totalCount));
  }
}
