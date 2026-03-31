using Server.Infrastructure;
using Server.UseCases.FeatureFlags.List;

namespace Server.Web.FeatureFlags.List;

public class ListFeatureFlags(IMediator mediator) : Endpoint<EmptyRequest, FeatureFlagsResponse, FeatureFlagsMapper>
{
  public override void Configure()
  {
    Get("/api/feature-flags");
    AllowAnonymous();
    Summary(s =>
    {
      s.Summary = "List client-visible feature flags";
      s.Description = "Returns feature flags visible to the client in v2 format.";
    });
  }

  public override async Task HandleAsync(EmptyRequest req, CancellationToken cancellationToken)
  {
    var result = await mediator.Send(new ListFeatureFlagsQuery(), cancellationToken);

    await Send.ResultMapperAsync(result, Map.FromEntityAsync, cancellationToken);
  }
}
