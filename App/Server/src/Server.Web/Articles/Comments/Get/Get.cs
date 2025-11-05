using Server.Core.ArticleAggregate.Dtos;
using Server.Core.Interfaces;
using Server.Infrastructure;
using Server.UseCases.Articles.Comments.Get;

namespace Server.Web.Articles.Comments.Get;

/// <summary>
/// Get article comments
/// </summary>
/// <remarks>
/// Get all comments for an article. Authentication optional.
/// </remarks>
public class Get(IMediator _mediator, IUserContext userContext) : Endpoint<GetCommentsRequest, CommentsResponse>
{
  public override void Configure()
  {
    Get("/api/articles/{slug}/comments");
    AllowAnonymous();
    Summary(s =>
    {
      s.Summary = "Get article comments";
      s.Description = "Get all comments for an article. Authentication optional.";
    });
  }

  public override async Task HandleAsync(GetCommentsRequest request, CancellationToken cancellationToken)
  {
    // Get current user ID if authenticated
    var currentUserId = userContext.GetCurrentUserId();

    var result = await _mediator.Send(new GetCommentsQuery(request.Slug, currentUserId), cancellationToken);

    await Send.ResultValueAsync(result, cancellationToken);
  }
}
