using Microsoft.Extensions.Logging;
using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Dtos;
using Server.Core.ArticleAggregate.Specifications.Articles;
using Server.Core.UserAggregate;
using Server.Core.UserAggregate.Specifications;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Persistence;

namespace Server.UseCases.Articles.Comments.Create;

public class CreateCommentHandler : ICommandHandler<CreateCommentCommand, CommentResponse>
{
  private readonly IRepository<Article> _articleRepository;
  private readonly IRepository<User> _userRepository;
  private readonly ILogger<CreateCommentHandler> _logger;

  public CreateCommentHandler(
    IRepository<Article> articleRepository,
    IRepository<User> userRepository,
    ILogger<CreateCommentHandler> logger)
  {
    _articleRepository = articleRepository;
    _userRepository = userRepository;
    _logger = logger;
  }

  public async Task<Result<CommentResponse>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
  {
    // Find the article
    var article = await _articleRepository.FirstOrDefaultAsync(new ArticleBySlugSpec(request.Slug), cancellationToken);
    if (article == null)
    {
      return Result<CommentResponse>.NotFound(typeof(Article), request.Slug);
    }

    // Find the user
    var user = await _userRepository.GetByIdAsync(request.AuthorId, cancellationToken);
    if (user == null)
    {
      return Result<CommentResponse>.ErrorMissingRequiredEntity(typeof(User), request.AuthorId);
    }

    // Create the comment
    var comment = new Comment(request.Body, user, article);

    // Add it to the article
    article.Comments.Add(comment);

    // Update the article to let EF Core track the change
    await _articleRepository.UpdateAsync(article, cancellationToken);

    // PV012: We need to call SaveChangesAsync here to populate database-generated values (Id, CreatedAt, UpdatedAt)
    // before returning the response. The UnitOfWork transaction is still open and will commit after this handler completes.
    // This is an exception to the rule because we need to return the comment with its timestamps in the response.
#pragma warning disable PV012
    await _articleRepository.SaveChangesAsync(cancellationToken);
#pragma warning restore PV012

    _logger.LogInformation("Comment created successfully with ID {CommentId}", comment.Id);

    // Check if current user is following the comment author
    var currentUser = request.CurrentUserId.HasValue ?
        await _userRepository.FirstOrDefaultAsync(new UserWithFollowingSpec(request.CurrentUserId.Value), cancellationToken) : null;

    // Return the comment response
    var response = new CommentResponse(CommentMappers.MapToDto(comment, currentUser));

    return Result<CommentResponse>.Created(response);
  }
}
