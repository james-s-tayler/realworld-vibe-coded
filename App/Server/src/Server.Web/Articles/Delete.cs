using Server.Core.Interfaces;
using Server.UseCases.Articles.Delete;

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

    if (result.IsSuccess)
    {
      await SendNoContentAsync(cancellationToken);
      return;
    }

    if (result.Status == ResultStatus.NotFound)
    {
      await SendAsync(new
      {
        errors = new { body = new[] { "Article not found" } }
      }, 404, cancellationToken);
      return;
    }

    if (result.Status == ResultStatus.Forbidden)
    {
      await SendAsync(new
      {
        errors = new { body = new[] { "You can only delete your own articles" } }
      }, 403, cancellationToken);
      return;
    }

    await SendAsync(new
    {
      errors = new { body = result.Errors.ToArray() }
    }, 400, cancellationToken);
  }
}
