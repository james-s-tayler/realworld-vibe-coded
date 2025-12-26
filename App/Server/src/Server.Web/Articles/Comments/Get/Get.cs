using Server.Core.ArticleAggregate.Dtos;
using Server.Infrastructure;
using Server.UseCases.Articles.Comments.Get;
using Server.UseCases.Interfaces;

namespace Server.Web.Articles.Comments.Get;

/// <summary>
/// Get article comments
/// </summary>
/// <remarks>
/// Get all comments for an article. Authentication required.
/// </remarks>
public class Get(IMediator mediator, IUserContext userContext) : Endpoint<GetCommentsRequest, CommentsResponse>
{
  public override void Configure()
  {
    Get("/api/articles/{slug}/comments");
    AuthSchemes(Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme, Microsoft.AspNetCore.Identity.IdentityConstants.BearerScheme);
    Summary(s =>
    {
      s.Summary = "Get article comments";
      s.Description = "Get all comments for an article. Authentication required.";
    });
  }

  public override async Task HandleAsync(GetCommentsRequest request, CancellationToken cancellationToken)
  {
    // Get current user ID (authentication required)
    var currentUserId = userContext.GetRequiredCurrentUserId();

    var result = await mediator.Send(new GetCommentsQuery(request.Slug, currentUserId), cancellationToken);

    await Send.ResultValueAsync(result, cancellationToken);
  }
}
