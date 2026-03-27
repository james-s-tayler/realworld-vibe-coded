using Server.Core.ArticleAggregate;
using Server.Core.CommentAggregate;
using Server.Core.UserFollowingAggregate;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Persistence;
using Server.UseCases.Articles.Specs;
using Server.UseCases.Comments.Specs;
using Server.UseCases.Profiles;

namespace Server.UseCases.Comments.List;

public class ListCommentsHandler(
  IReadRepository<Article> articleRepo,
  IReadRepository<Comment> commentRepo,
  IReadRepository<UserFollowing> followingRepo)
  : IQueryHandler<ListCommentsQuery, CommentsListResult>
{
  public async Task<Result<CommentsListResult>> Handle(ListCommentsQuery request, CancellationToken cancellationToken)
  {
    var article = await articleRepo.FirstOrDefaultAsync(
      new ArticleBySlugSpec(request.Slug), cancellationToken);

    if (article == null)
    {
      return Result<CommentsListResult>.Error(
        new SharedKernel.Result.ErrorDetail("slug", "Article not found."));
    }

    var comments = await commentRepo.ListAsync(
      new CommentsByArticleSpec(article.Id), cancellationToken);

    var results = new List<CommentResult>();
    foreach (var comment in comments)
    {
      var authorFollowing = await followingRepo.AnyAsync(
        new UserFollowingByUsersSpec(request.CurrentUserId, comment.AuthorId),
        cancellationToken);
      results.Add(new CommentResult(comment, authorFollowing));
    }

    return Result<CommentsListResult>.Success(new CommentsListResult(results));
  }
}
