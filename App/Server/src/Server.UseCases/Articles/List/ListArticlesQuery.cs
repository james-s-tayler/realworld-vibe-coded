using Server.Core.ArticleAggregate;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Pagination;

namespace Server.UseCases.Articles.List;

public record ListArticlesQuery(
  string? Tag = null,
  string? Author = null,
  string? Favorited = null,
  int Limit = 20,
  int Offset = 0,
  Guid? CurrentUserId = null
) : IQuery<PagedResult<Article>>;
