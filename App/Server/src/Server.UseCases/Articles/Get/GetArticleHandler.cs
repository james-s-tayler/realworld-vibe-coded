using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Specifications;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Persistence;

namespace Server.UseCases.Articles.Get;

public class GetArticleHandler(IRepository<Article> _articleRepository)
  : IQueryHandler<GetArticleQuery, Article>
{
  public async Task<Result<Article>> Handle(GetArticleQuery request, CancellationToken cancellationToken)
  {
    var article = await _articleRepository.FirstOrDefaultAsync(
      new ArticleBySlugSpec(request.Slug), cancellationToken);

    if (article == null)
    {
      return Result<Article>.NotFound(new ErrorDetail("NotFound", "Article not found"));
    }

    return Result<Article>.Success(article);
  }
}
