using Server.UseCases.Tags;
using Server.UseCases.Tags.List;
using Server.Web.Infrastructure;

namespace Server.Web.Tags.List;

/// <summary>
/// Get all tags
/// </summary>
/// <remarks>
/// Get all tags used in articles. No authentication required.
/// </remarks>
public class List(IMediator _mediator) : EndpointWithoutRequest<TagsResponse>
{
  public override void Configure()
  {
    Get("/api/tags");
    AllowAnonymous();
    Summary(s =>
    {
      s.Summary = "Get all tags";
      s.Description = "Get all tags used in articles. No authentication required.";
    });
  }

  public override async Task HandleAsync(CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(new ListTagsQuery(), cancellationToken);

    await Send.ResultAsync(result, cancellationToken);
  }
}
