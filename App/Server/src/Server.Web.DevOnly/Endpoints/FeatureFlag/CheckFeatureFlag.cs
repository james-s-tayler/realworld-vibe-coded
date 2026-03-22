using Server.Web.DevOnly.UseCases.FeatureFlag;

namespace Server.Web.DevOnly.Endpoints.FeatureFlag;

public class CheckFeatureFlag(IMediator mediator) : Endpoint<CheckFeatureFlagRequest, CheckFeatureFlagResponse>
{
  public override void Configure()
  {
    Get("feature-flags/{FeatureName}");
    Group<TestData>();
    Summary(s =>
    {
      s.Summary = "Check feature flag status";
      s.Description = "Returns whether a feature flag is enabled or disabled.";
    });
  }

  public override async Task HandleAsync(CheckFeatureFlagRequest req, CancellationToken ct)
  {
    var result = await mediator.Send(new CheckFeatureFlagQuery(req.FeatureName), ct);
    await Send.ResultValueAsync(result, ct);
  }
}
