using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Specifications.Articles;
using Server.Core.AuthorAggregate;
using Server.Core.AuthorAggregate.Specifications;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Persistence;

namespace Server.UseCases.Articles.Feed;

public class GetFeedHandler(
  IReadRepository<Author> authorRepository,
  IReadRepository<Article> articleRepository)
  : IQueryHandler<GetFeedQuery, GetFeedResult>
{
  public async Task<Result<GetFeedResult>> Handle(GetFeedQuery request, CancellationToken cancellationToken)
  {
    // Get the author with their following relationships
    var author = await authorRepository.FirstOrDefaultAsync(
      new AuthorWithFollowingByUserIdSpec(request.UserId), cancellationToken);

    if (author == null)
    {
      return Result<GetFeedResult>.Success(new GetFeedResult(new List<Article>(), 0));
    }

    // Get IDs of authors that the current user follows
    var followedUserIds = author.Following
      .Select(af => af.FollowedId)
      .ToList();

    // If not following anyone, return empty list
    if (!followedUserIds.Any())
    {
      return Result<GetFeedResult>.Success(new GetFeedResult(new List<Article>(), 0));
    }

    // Get articles from followed users using specification
    var spec = new FeedArticlesSpec(followedUserIds, request.Limit, request.Offset);
    var articles = await articleRepository.ListAsync(spec, cancellationToken);

    // Get total count without pagination
    var countSpec = new CountFeedArticlesSpec(followedUserIds);
    var totalCount = await articleRepository.CountAsync(countSpec, cancellationToken);

    return Result<GetFeedResult>.Success(new GetFeedResult(articles, totalCount));
  }
}
