using Ardalis.Result;
using Ardalis.SharedKernel;
using MediatR;
using Microsoft.Extensions.Logging;
using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Specifications;

namespace Server.UseCases.Articles.Comments.Delete;

public class DeleteCommentHandler : IRequestHandler<DeleteCommentCommand, Result>
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

  public async Task<Result> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
  {
    _logger.LogInformation("Handling DeleteCommentCommand");
    _logger.LogInformation("Property Slug : {Slug}", request.Slug);
    _logger.LogInformation("Property CommentId : {CommentId}", request.CommentId);
    _logger.LogInformation("Property UserId : {UserId}", request.UserId);

    // Find the article
    var article = await _articleRepository.FirstOrDefaultAsync(new ArticleBySlugSpec(request.Slug), cancellationToken);
    if (article == null)
    {
      return Result.NotFound("Article not found");
    }

    // Find the comment
    var comment = article.Comments.FirstOrDefault(c => c.Id == request.CommentId);
    if (comment == null)
    {
      return Result.NotFound("Comment not found");
    }

    // Check if the user is the author of the comment
    if (comment.AuthorId != request.UserId)
    {
      return Result.Forbidden("You can only delete your own comments");
    }

    // Remove the comment
    article.Comments.Remove(comment);
    await _articleRepository.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("Comment {CommentId} deleted successfully", request.CommentId);

    return Result.Success();
  }
}
