using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Specifications.Articles;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Pagination;
using Server.SharedKernel.Persistence;

namespace Server.UseCases.Articles.List;

public class ListArticlesHandler(IReadRepository<Article> articleRepository)
  : IQueryHandler<ListArticlesQuery, PagedResult<Article>>
{
  public async Task<Result<PagedResult<Article>>> Handle(ListArticlesQuery request, CancellationToken cancellationToken)
  {
    var spec = new ListArticlesSpec(
      request.Tag,
      request.Author,
      request.Favorited,
      request.Limit,
      request.Offset);

    var articles = await articleRepository.ListAsync(spec, cancellationToken);
    var totalCount = await articleRepository.CountAsync(spec, cancellationToken);

    return Result<PagedResult<Article>>.Success(new PagedResult<Article>(articles, totalCount));
  }
}
