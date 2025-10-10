using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Specifications;
using Server.Core.UserAggregate;
using Server.Core.UserAggregate.Specifications;

namespace Server.UseCases.Articles.Get;

public class GetArticleHandler(
  IRepository<Article> _articleRepository,
  IRepository<User> _userRepository)
  : IQueryHandler<GetArticleQuery, Result<ArticleResponse>>
{
  public async Task<Result<ArticleResponse>> Handle(GetArticleQuery request, CancellationToken cancellationToken)
  {
    var article = await _articleRepository.FirstOrDefaultAsync(
      new ArticleBySlugSpec(request.Slug), cancellationToken);

    if (article == null)
    {
      return Result.NotFound("Article not found");
    }

    // Get current user with following relationships if authenticated
    var currentUser = request.CurrentUserId.HasValue ?
        await _userRepository.FirstOrDefaultAsync(new UserWithFollowingSpec(request.CurrentUserId.Value), cancellationToken) : null;

    // Use FastEndpoints-style mapper to convert entity to response
    var mapper = new ArticleResponseMapper(currentUser);
    var response = mapper.FromEntity(article);

    return Result.Success(response);
  }
}
