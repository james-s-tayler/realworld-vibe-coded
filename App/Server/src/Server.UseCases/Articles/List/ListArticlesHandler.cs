using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Specifications.Articles;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Persistence;

namespace Server.UseCases.Articles.List;

public class ListArticlesHandler(IReadRepository<Article> articleRepository)
  : IQueryHandler<ListArticlesQuery, ListArticlesResult>
{
  public async Task<Result<ListArticlesResult>> Handle(ListArticlesQuery request, CancellationToken cancellationToken)
  {
    var spec = new ListArticlesSpec(
      request.Tag,
      request.Author,
      request.Favorited,
      request.Limit,
      request.Offset);

    var articles = await articleRepository.ListAsync(spec, cancellationToken);

    // Get total count without pagination
    var countSpec = new CountArticlesSpec(
      request.Tag,
      request.Author,
      request.Favorited);

    var totalCount = await articleRepository.CountAsync(countSpec, cancellationToken);

    return Result<ListArticlesResult>.Success(new ListArticlesResult(articles, totalCount));
  }
}
