using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Specifications.Articles;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Persistence;

namespace Server.UseCases.Articles.List;

public class ListArticlesHandler(IReadRepository<Article> articleRepository)
  : IQueryHandler<ListArticlesQuery, (IEnumerable<Article> Articles, int TotalCount)>
{
  public async Task<Result<(IEnumerable<Article> Articles, int TotalCount)>> Handle(ListArticlesQuery request, CancellationToken cancellationToken)
  {
    var spec = new ListArticlesSpec(
      request.Tag,
      request.Author,
      request.Favorited,
      request.Limit,
      request.Offset);

    var countSpec = new ListArticlesCountSpec(
      request.Tag,
      request.Author,
      request.Favorited);

    var articles = await articleRepository.ListAsync(spec, cancellationToken);
    var totalCount = await articleRepository.CountAsync(countSpec, cancellationToken);

    return Result<(IEnumerable<Article> Articles, int TotalCount)>.Success((articles, totalCount));
  }
}
