using Server.Infrastructure;
using Server.UseCases.Articles.Delete;
using Server.UseCases.Interfaces;

namespace Server.Web.Articles.Delete;

/// <summary>
/// Delete article
/// </summary>
/// <remarks>
/// Deletes an existing article. Authentication required. User must be the author.
/// </remarks>
public class Delete(IMediator mediator, IUserContext userContext) : Endpoint<DeleteArticleRequest>
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
    var userId = userContext.GetRequiredCurrentUserId();

    var result = await mediator.Send(new DeleteArticleCommand(request.Slug, userId), cancellationToken);

    await Send.ResultValueAsync(result, cancellationToken);
  }
}
