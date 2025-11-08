using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Specifications.Articles;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Persistence;

namespace Server.UseCases.Articles.Get;

public class GetArticleHandler(IRepository<Article> articleRepository)
  : IQueryHandler<GetArticleQuery, Article>
{
  public async Task<Result<Article>> Handle(GetArticleQuery request, CancellationToken cancellationToken)
  {
    var article = await articleRepository.FirstOrDefaultAsync(
      new ArticleBySlugSpec(request.Slug), cancellationToken);

    if (article == null)
    {
      return Result<Article>.NotFound(request.Slug);
    }

    return Result<Article>.Success(article);
  }
}
