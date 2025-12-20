using Server.Core.ArticleAggregate.Dtos;
using Server.Infrastructure;
using Server.UseCases.Articles.Comments.Create;
using Server.UseCases.Interfaces;

namespace Server.Web.Articles.Comments.Create;

/// <summary>
/// Create comment for an article
/// </summary>
/// <remarks>
/// Create a new comment for an article. Authentication required.
/// </remarks>
public class Create(IMediator mediator, IUserContext userContext) : Endpoint<CreateCommentRequest, CommentResponse>
{
  public override void Configure()
  {
    Post("/api/articles/{slug}/comments");
    AuthSchemes("Token", Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme, Microsoft.AspNetCore.Identity.IdentityConstants.BearerScheme);
    Summary(s =>
    {
      s.Summary = "Create comment";
      s.Description = "Create a new comment for an article. Authentication required.";
    });
  }

  public override async Task HandleAsync(CreateCommentRequest request, CancellationToken cancellationToken)
  {
    var userId = userContext.GetRequiredCurrentUserId();

    // Get slug from route parameter
    var slug = Route<string>("slug") ?? string.Empty;

    var result = await mediator.Send(new CreateCommentCommand(slug, request.Comment.Body, userId, userId), cancellationToken);

    await Send.ResultValueAsync(result, cancellationToken);
  }
}
