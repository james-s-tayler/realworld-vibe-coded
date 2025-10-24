using Server.Core.Interfaces;
using Server.UseCases.Articles.Delete;
using Server.Web.Infrastructure;

namespace Server.Web.Articles.Delete;

/// <summary>
/// Delete article
/// </summary>
/// <remarks>
/// Deletes an existing article. Authentication required. User must be the author.
/// </remarks>
public class Delete(IMediator _mediator, ICurrentUserService _currentUserService) : Endpoint<DeleteArticleRequest>
{
  public override void Configure()
  {
    Delete("/api/articles/{slug}");
    AuthSchemes("Token");
    Summary(s =>
    {
      s.Summary = "Delete article";
      s.Description = "Deletes an existing article. Authentication required. User must be the author.";
    });
  }

  public override async Task HandleAsync(DeleteArticleRequest request, CancellationToken cancellationToken)
  {
    var userId = _currentUserService.GetRequiredCurrentUserId();

    var result = await _mediator.Send(new DeleteArticleCommand(request.Slug, userId), cancellationToken);

    await Send.ResultValueAsync(result, cancellationToken);
  }
}
