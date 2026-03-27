using Server.Infrastructure;
using Server.UseCases.Comments.Create;
using Server.UseCases.Interfaces;

namespace Server.Web.Comments.Create;

public class Create(IMediator mediator, IUserContext userContext) : Endpoint<CreateCommentRequest, CommentResponse, CommentMapper>
{
  public override void Configure()
  {
    Post("/api/articles/{slug}/comments");
    AuthSchemes(Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme, Microsoft.AspNetCore.Identity.IdentityConstants.BearerScheme);
  }

  public override async Task HandleAsync(CreateCommentRequest request, CancellationToken cancellationToken)
  {
    var userId = userContext.GetRequiredCurrentUserId();

    var result = await mediator.Send(
      new CreateCommentCommand(request.Slug, request.Comment.Body, userId),
      cancellationToken);

    await Send.ResultMapperAsync(
      result,
      async (commentResult, ct) => await Map.FromEntityAsync(commentResult, ct),
      cancellationToken);
  }
}
