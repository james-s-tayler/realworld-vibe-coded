using Server.Core.Interfaces;
using Server.UseCases.Articles.Delete;
using Server.Web.Infrastructure;

namespace Server.Web.Articles;

/// <summary>
/// Delete article
/// </summary>
/// <remarks>
/// Deletes an existing article. Authentication required. User must be the author.
/// </remarks>
public class Delete(IMediator _mediator, ICurrentUserService _currentUserService) : EndpointWithoutRequest
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

  public override async Task HandleAsync(CancellationToken cancellationToken)
  {
    var userId = _currentUserService.GetRequiredCurrentUserId();

    // Get slug from route
    var slug = Route<string>("slug") ?? string.Empty;

    var result = await _mediator.Send(new DeleteArticleCommand(slug, userId), cancellationToken);

    await Send.ResultAsync(result, cancellationToken);
  }
}
