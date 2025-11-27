using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Specifications.Articles;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Persistence;

namespace Server.UseCases.Articles.List;

public class ListArticlesHandler(IReadRepository<Article> articleRepository)
  : IQueryHandler<ListArticlesQuery, IEnumerable<Article>>
{
  public async Task<Result<IEnumerable<Article>>> Handle(ListArticlesQuery request, CancellationToken cancellationToken)
  {
    var spec = new ListArticlesSpec(
      request.Tag,
      request.Author,
      request.Favorited,
      request.Limit,
      request.Offset);

    var articles = await articleRepository.ListAsync(spec, cancellationToken);

    return Result<IEnumerable<Article>>.Success(articles);
  }
}
