using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Dtos;
using Server.Core.ArticleAggregate.Specifications.Articles;
using Server.Core.IdentityAggregate;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Persistence;

namespace Server.UseCases.Articles.Comments.Create;

public class CreateCommentHandler : ICommandHandler<CreateCommentCommand, CommentResponse>
{
  private readonly IRepository<Article> _articleRepository;
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly ILogger<CreateCommentHandler> _logger;

  public CreateCommentHandler(
    IRepository<Article> articleRepository,
    UserManager<ApplicationUser> userManager,
    ILogger<CreateCommentHandler> logger)
  {
    _articleRepository = articleRepository;
    _userManager = userManager;
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
    var user = await _userManager.FindByIdAsync(request.AuthorId.ToString());
    if (user == null)
    {
      return Result<CommentResponse>.ErrorMissingRequiredEntity(typeof(ApplicationUser), request.AuthorId);
    }

    // Create the comment
    var comment = new Comment(request.Body, user, article);

    // Add it to the article
    article.Comments.Add(comment);

    // Update the article to let EF Core track the change
    await _articleRepository.UpdateAsync(article, cancellationToken);

    _logger.LogInformation("Comment created successfully with ID {CommentId}", comment.Id);

    // Check if current user is following the comment author
    ApplicationUser? currentUser = null;
    if (request.CurrentUserId.HasValue)
    {
      currentUser = await _userManager.Users
        .Include(u => u.Following)
        .FirstOrDefaultAsync(u => u.Id == request.CurrentUserId.Value, cancellationToken);
    }

    // Return the comment response
    var response = new CommentResponse(CommentMappers.MapToDto(comment, currentUser));

    return Result<CommentResponse>.Created(response);
  }
}
