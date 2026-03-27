using Server.Infrastructure;
using Server.UseCases.Comments.List;
using Server.UseCases.Interfaces;

namespace Server.Web.Comments.List;

public class List(IMediator mediator, IUserContext userContext) : Endpoint<ListCommentsRequest, CommentsResponse, CommentsMapper>
{
  public override void Configure()
  {
    Get("/api/articles/{slug}/comments");
    AuthSchemes(Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme, Microsoft.AspNetCore.Identity.IdentityConstants.BearerScheme);
  }

  public override async Task HandleAsync(ListCommentsRequest request, CancellationToken cancellationToken)
  {
    var userId = userContext.GetRequiredCurrentUserId();

    var result = await mediator.Send(
      new ListCommentsQuery(request.Slug, userId),
      cancellationToken);

    await Send.ResultMapperAsync(
      result,
      async (listResult, ct) => await Map.FromEntityAsync(listResult, ct),
      cancellationToken);
  }
}
