using Server.Core.ArticleAggregate;
using Server.Core.CommentAggregate;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Persistence;
using Server.UseCases.Articles.Specs;

namespace Server.UseCases.Comments.Delete;

public class DeleteCommentHandler(
  IReadRepository<Article> articleRepo,
  IRepository<Comment> commentRepo)
  : ICommandHandler<DeleteCommentCommand, bool>
{
  public async Task<Result<bool>> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
  {
    var article = await articleRepo.FirstOrDefaultAsync(
      new ArticleBySlugSpec(request.Slug), cancellationToken);

    if (article == null)
    {
      return Result<bool>.Error(new ErrorDetail("slug", "Article not found."));
    }

    if (!Guid.TryParse(request.CommentId, out var commentId))
    {
      return Result<bool>.Invalid(new ErrorDetail("id", "Invalid comment ID format."));
    }

    var comment = await commentRepo.GetByIdAsync(commentId, cancellationToken);

    if (comment == null)
    {
      return Result<bool>.NotFound();
    }

    if (comment.ArticleId != article.Id)
    {
      return Result<bool>.NotFound();
    }

    if (comment.AuthorId != request.CurrentUserId)
    {
      return Result<bool>.Forbidden();
    }

    await commentRepo.DeleteAsync(comment, cancellationToken);

    return Result<bool>.NoContent();
  }
}
