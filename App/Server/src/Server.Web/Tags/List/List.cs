using Server.Infrastructure;
using Server.UseCases.Tags;
using Server.UseCases.Tags.List;

namespace Server.Web.Tags.List;

/// <summary>
/// Get all tags
/// </summary>
/// <remarks>
/// Get all tags used in articles. Authentication required.
/// </remarks>
public class List(IMediator mediator) : Endpoint<EmptyRequest, TagsResponse>
{
  public override void Configure()
  {
    Get("/api/tags");
    AuthSchemes(Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme, Microsoft.AspNetCore.Identity.IdentityConstants.BearerScheme);
    Summary(s =>
    {
      s.Summary = "Get all tags";
      s.Description = "Get all tags used in articles. Authentication required.";
    });
  }

  public override async Task HandleAsync(EmptyRequest request, CancellationToken cancellationToken)
  {
    var result = await mediator.Send(new ListTagsQuery(), cancellationToken);

    await Send.ResultValueAsync(result, cancellationToken);
  }
}
