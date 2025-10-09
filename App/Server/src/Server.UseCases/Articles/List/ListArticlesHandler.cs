using Server.Core.Interfaces;
using Server.Core.UserAggregate;
using Server.Core.UserAggregate.Specifications;

namespace Server.UseCases.Articles.List;

public class ListArticlesHandler(
  IListArticlesQueryService _query,
  IRepository<User> _userRepository)
  : IQueryHandler<ListArticlesQuery, Result<ArticlesResponse>>
{
  public async Task<Result<ArticlesResponse>> Handle(ListArticlesQuery request, CancellationToken cancellationToken)
  {
    var articles = await _query.ListAsync(
      request.Tag,
      request.Author,
      request.Favorited,
      request.Limit,
      request.Offset);

    // Get current user with following relationships if authenticated
    User? currentUser = null;
    if (request.CurrentUserId.HasValue)
    {
      currentUser = await _userRepository.FirstOrDefaultAsync(
        new UserWithFollowingSpec(request.CurrentUserId.Value), cancellationToken);
    }

    // Map entities to DTOs in the Application layer
    var articleDtos = articles.Select(a => ArticleMappers.MapToDto(a, currentUser)).ToList();
    var articlesCount = articleDtos.Count;

    return Result.Success(new ArticlesResponse(articleDtos, articlesCount));
  }
}
