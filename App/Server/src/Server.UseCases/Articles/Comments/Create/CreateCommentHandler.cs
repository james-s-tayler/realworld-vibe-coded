using Microsoft.Extensions.Logging;
using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Dtos;
using Server.Core.ArticleAggregate.Specifications.Articles;
using Server.Core.AuthorAggregate;
using Server.Core.AuthorAggregate.Specifications;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Persistence;

namespace Server.UseCases.Articles.Comments.Create;

public class CreateCommentHandler(
  IRepository<Article> articleRepository,
  IRepository<Author> authorRepository,
  ILogger<CreateCommentHandler> logger)
  : ICommandHandler<CreateCommentCommand, CommentResponse>
{
  public async Task<Result<CommentResponse>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
  {
    // Find the article
    var article = await articleRepository.FirstOrDefaultAsync(new ArticleBySlugSpec(request.Slug), cancellationToken);
    if (article == null)
    {
      return Result<CommentResponse>.NotFound(typeof(Article), request.Slug);
    }

    // Get the author (domain invariant - must exist)
    var author = await authorRepository.FirstOrDefaultAsync(
      new AuthorByUserIdSpec(request.AuthorId), cancellationToken);

    if (author == null)
    {
      return Result<CommentResponse>.ErrorMissingRequiredEntity(typeof(Author), request.AuthorId);
    }

    // Create the comment
    var comment = new Comment(request.Body, author, article);

    // Add it to the article
    article.Comments.Add(comment);

    // Update the article to let EF Core track the change
    await articleRepository.UpdateAsync(article, cancellationToken);

    logger.LogInformation("Comment created successfully with ID {CommentId}", comment.Id);

    // Check if current user is following the comment author via AuthorFollowing
    bool isFollowing = false;
    if (request.CurrentUserId.HasValue)
    {
      var currentAuthor = await authorRepository.FirstOrDefaultAsync(
        new AuthorWithFollowingByUserIdSpec(request.CurrentUserId.Value), cancellationToken);
      isFollowing = currentAuthor?.IsFollowing(comment.AuthorId) ?? false;
    }

    var response = new CommentResponse(CommentMappers.MapToDto(comment, isFollowing));

    return Result<CommentResponse>.Created(response);
  }
}
