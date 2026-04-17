using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Specifications.Articles;
using Server.Core.AuthorAggregate;
using Server.Core.AuthorAggregate.Specifications;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Pagination;
using Server.SharedKernel.Persistence;

namespace Server.UseCases.Articles.Feed;

public class GetFeedHandler(
  IReadRepository<Author> authorRepository,
  IReadRepository<Article> articleRepository)
  : IQueryHandler<GetFeedQuery, PagedResult<Article>>
{
  public async Task<Result<PagedResult<Article>>> Handle(GetFeedQuery request, CancellationToken cancellationToken)
  {
    var author = await authorRepository.FirstOrDefaultAsync(
      new AuthorWithFollowingByUserIdSpec(request.UserId), cancellationToken);

    if (author == null)
    {
      return Result<PagedResult<Article>>.Success(new PagedResult<Article>([], 0));
    }

    var followedUserIds = author.Following
      .Select(af => af.FollowedId)
      .ToList();

    if (followedUserIds.Count == 0)
    {
      return Result<PagedResult<Article>>.Success(new PagedResult<Article>([], 0));
    }

    var spec = new FeedArticlesSpec(followedUserIds, request.Limit, request.Offset);
    var articles = await articleRepository.ListAsync(spec, cancellationToken);
    var totalCount = await articleRepository.CountAsync(spec, cancellationToken);

    return Result<PagedResult<Article>>.Success(new PagedResult<Article>(articles, totalCount));
  }
}
