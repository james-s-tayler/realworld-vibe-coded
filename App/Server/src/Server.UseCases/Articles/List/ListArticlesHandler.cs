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

    // Use FastEndpoints-style mapper to convert entities to response
    var mapper = new ArticleResponseMapper(currentUser);
    var response = mapper.FromEntities(articles);

    return Result.Success(response);
  }
}
