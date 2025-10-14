using Server.Core.ArticleAggregate.Dtos;
using Server.Core.Interfaces;
using Server.UseCases.Articles.Comments.Create;
using Server.Web.Infrastructure;

namespace Server.Web.Articles.Comments;

/// <summary>
/// Create comment for an article
/// </summary>
/// <remarks>
/// Create a new comment for an article. Authentication required.
/// </remarks>
public class Create(IMediator _mediator, ICurrentUserService _currentUserService) : Endpoint<CreateCommentRequest, CommentResponse>
{
  public override void Configure()
  {
    Post("/api/articles/{slug}/comments");
    AuthSchemes("Token");
    Summary(s =>
    {
      s.Summary = "Create comment";
      s.Description = "Create a new comment for an article. Authentication required.";
    });
  }

  public override async Task HandleAsync(CreateCommentRequest request, CancellationToken cancellationToken)
  {
    var userId = _currentUserService.GetRequiredCurrentUserId();

    // Get slug from route parameter
    var slug = Route<string>("slug") ?? string.Empty;

    var result = await _mediator.Send(new CreateCommentCommand(slug, request.Comment.Body, userId, userId), cancellationToken);

    await this.SendAsync(result, cancellationToken, treatNotFoundAsValidation: true);
  }
}

public class CreateCommentRequest
{
  public CreateCommentDto Comment { get; set; } = default!;
}

public class CreateCommentDto
{
  public string Body { get; set; } = string.Empty;
}
