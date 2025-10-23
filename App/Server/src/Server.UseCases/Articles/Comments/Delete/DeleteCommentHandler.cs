using MediatR;
using Microsoft.Extensions.Logging;
using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Specifications;

namespace Server.UseCases.Articles.Comments.Delete;

public class DeleteCommentHandler : IRequestHandler<DeleteCommentCommand, Result<Unit>>
{
  private readonly IRepository<Article> _articleRepository;
  private readonly ILogger<DeleteCommentHandler> _logger;

  public DeleteCommentHandler(
    IRepository<Article> articleRepository,
    ILogger<DeleteCommentHandler> logger)
  {
    _articleRepository = articleRepository;
    _logger = logger;
  }

  public async Task<Result<Unit>> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
  {
    // Find the article
    var article = await _articleRepository.FirstOrDefaultAsync(new ArticleBySlugSpec(request.Slug), cancellationToken);
    if (article == null)
    {
      return Result<Unit>.NotFound("Article not found");
    }

    // Find the comment
    var comment = article.Comments.FirstOrDefault(c => c.Id == request.CommentId);
    if (comment == null)
    {
      return Result<Unit>.NotFound("Comment not found");
    }

    // Check if the user is the author of the comment
    if (comment.AuthorId != request.UserId)
    {
      return Result<Unit>.Forbidden("You can only delete your own comments");
    }

    // Remove the comment
    article.Comments.Remove(comment);
    await _articleRepository.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("Comment {CommentId} deleted successfully", request.CommentId);

    return Result<Unit>.Success(Unit.Value);
  }
}
