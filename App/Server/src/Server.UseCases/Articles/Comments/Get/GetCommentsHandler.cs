using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Dtos;
using Server.Core.ArticleAggregate.Specifications;
using Server.Core.UserAggregate;
using Server.Core.UserAggregate.Specifications;

namespace Server.UseCases.Articles.Comments.Get;

public class GetCommentsHandler(IRepository<Article> _articleRepository, IRepository<User> _userRepository)
  : IQueryHandler<GetCommentsQuery, CommentsResponse>
{
  public async Task<Result<CommentsResponse>> Handle(GetCommentsQuery request, CancellationToken cancellationToken)
  {
    var article = await _articleRepository.FirstOrDefaultAsync(
      new ArticleBySlugSpec(request.Slug), cancellationToken);

    if (article == null)
    {
      return Result.NotFound("Article not found");
    }

    // Get current user with following relationships if authenticated
    User? currentUser = null;
    if (request.CurrentUserId.HasValue)
    {
      currentUser = await _userRepository.FirstOrDefaultAsync(
        new UserWithFollowingSpec(request.CurrentUserId.Value), cancellationToken);
    }

    var commentDtos = article.Comments.Select(c => CommentMappers.MapToDto(c, currentUser)).ToList();

    return Result.Success(new CommentsResponse(commentDtos));
  }
}
