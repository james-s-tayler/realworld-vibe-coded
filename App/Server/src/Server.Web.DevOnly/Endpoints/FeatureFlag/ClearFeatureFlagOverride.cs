using Server.Web.DevOnly.UseCases.FeatureFlag;

namespace Server.Web.DevOnly.Endpoints.FeatureFlag;

public class ClearFeatureFlagOverride(IMediator mediator) : Endpoint<CheckFeatureFlagRequest, CheckFeatureFlagResponse>
{
  public override void Configure()
  {
    Delete("{FeatureName}");
    Group<FeatureFlagDevOnly>();
    Summary(s =>
    {
      s.Summary = "Clear a feature flag override";
      s.Description = "Removes a runtime override, reverting to the configured value. Only available in Development environment.";
    });
  }

  public override async Task HandleAsync(CheckFeatureFlagRequest req, CancellationToken ct)
  {
    var result = await mediator.Send(new ClearFeatureFlagOverrideCommand(req.FeatureName), ct);
    await Send.ResultValueAsync(result, ct);
  }
}
