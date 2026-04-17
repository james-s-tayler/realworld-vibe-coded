using Server.Core.ArticleAggregate;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Pagination;

namespace Server.UseCases.Articles.Feed;

public record GetFeedQuery(
  Guid UserId,
  int Limit = 20,
  int Offset = 0
) : IQuery<PagedResult<Article>>;
