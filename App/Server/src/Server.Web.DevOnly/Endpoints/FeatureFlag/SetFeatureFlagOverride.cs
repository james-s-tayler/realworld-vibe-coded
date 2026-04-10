using Server.Web.DevOnly.UseCases.FeatureFlag;

namespace Server.Web.DevOnly.Endpoints.FeatureFlag;

public class SetFeatureFlagOverride(IMediator mediator) : Endpoint<SetFeatureFlagOverrideRequest, CheckFeatureFlagResponse>
{
  public override void Configure()
  {
    Put("{FeatureName}");
    Group<FeatureFlagDevOnly>();
    Summary(s =>
    {
      s.Summary = "Override a feature flag value";
      s.Description = "Sets a runtime override for a feature flag. Only available in Development environment.";
    });
  }

  public override async Task HandleAsync(SetFeatureFlagOverrideRequest req, CancellationToken ct)
  {
    var result = await mediator.Send(new SetFeatureFlagOverrideCommand(req.FeatureName, req.Enabled), ct);
    await Send.ResultValueAsync(result, ct);
  }
}
