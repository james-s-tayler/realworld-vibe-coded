using Ardalis.Result;
using Ardalis.SharedKernel;
using MediatR;
using Microsoft.Extensions.Logging;
using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Dtos;
using Server.Core.ArticleAggregate.Specifications;
using Server.Core.UserAggregate;

namespace Server.UseCases.Articles.Comments.Create;

public class CreateCommentHandler : IRequestHandler<CreateCommentCommand, Result<CommentResponse>>
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
    _logger.LogInformation("Handling CreateCommentCommand");
    _logger.LogInformation("Property Slug : {Slug}", request.Slug);
    _logger.LogInformation("Property Body : {Body}", request.Body);
    _logger.LogInformation("Property AuthorId : {AuthorId}", request.AuthorId);

    // Find the article
    var article = await _articleRepository.FirstOrDefaultAsync(new ArticleBySlugSpec(request.Slug), cancellationToken);
    if (article == null)
    {
      return Result.NotFound("Article not found");
    }

    // Find the user
    var user = await _userRepository.GetByIdAsync(request.AuthorId, cancellationToken);
    if (user == null)
    {
      return Result.NotFound("User not found");
    }

    // Create the comment
    var comment = new Comment(request.Body, user, article);

    // Add it to the article
    article.Comments.Add(comment);
    await _articleRepository.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("Comment created successfully with ID {CommentId}", comment.Id);

    // Return the comment response
    var response = new CommentResponse(
      new CommentDto(
        comment.Id,
        comment.CreatedAt,
        comment.UpdatedAt,
        comment.Body,
        new AuthorDto(
          user.Username,
          user.Bio ?? string.Empty,
          user.Image,
          false // TODO: Implement following logic
        )
      )
    );

    return Result.Success(response);
  }
}
