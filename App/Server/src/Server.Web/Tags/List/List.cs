using Server.Infrastructure;
using Server.UseCases.Tags.List;

namespace Server.Web.Tags.List;

public class List(IMediator mediator) : Endpoint<EmptyRequest, TagsResponse, TagsMapper>
{
  public override void Configure()
  {
    Get("/api/tags");
    AuthSchemes(Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme, Microsoft.AspNetCore.Identity.IdentityConstants.BearerScheme);
  }

  public override async Task HandleAsync(EmptyRequest request, CancellationToken cancellationToken)
  {
    var result = await mediator.Send(new ListTagsQuery(), cancellationToken);

    await Send.ResultMapperAsync(
      result,
      async (tags, ct) => await Map.FromEntityAsync(tags, ct),
      cancellationToken);
  }
}
