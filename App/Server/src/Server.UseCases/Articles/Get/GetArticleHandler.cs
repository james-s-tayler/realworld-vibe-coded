using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Specifications;

namespace Server.UseCases.Articles.Get;

public class GetArticleHandler(
  IRepository<Article> _articleRepository)
  : IQueryHandler<GetArticleQuery, Result<Article>>
{
  public async Task<Result<Article>> Handle(GetArticleQuery request, CancellationToken cancellationToken)
  {
    var article = await _articleRepository.FirstOrDefaultAsync(
      new ArticleBySlugSpec(request.Slug), cancellationToken);

    if (article == null)
    {
      return Result.NotFound("Article not found");
    }

    return Result.Success(article);
  }
}
