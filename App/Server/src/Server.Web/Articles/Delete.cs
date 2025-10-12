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

    if (result.IsSuccess)
    {
      await SendNoContentAsync();
      return;
    }

    if (result.Status == ResultStatus.NotFound)
    {
      await HttpContext.Response.HttpContext.Response.SendAsync(new ConduitErrorResponse
      {
        Errors = new ConduitErrorBody { Body = new[] { "Article not found" } }
      }, 404);
      return;
    }

    if (result.Status == ResultStatus.Forbidden)
    {
      await HttpContext.Response.HttpContext.Response.SendAsync(new ConduitErrorResponse
      {
        Errors = new ConduitErrorBody { Body = new[] { "You can only delete your own articles" } }
      }, 403);
      return;
    }

    await HttpContext.Response.HttpContext.Response.SendAsync(new ConduitErrorResponse
    {
      Errors = new ConduitErrorBody { Body = result.Errors.ToArray() }
    }, 400);
  }
}
