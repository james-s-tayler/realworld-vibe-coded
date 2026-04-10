using Server.UseCases.FeatureFlags.List;

namespace Server.Web.FeatureFlags.List;

public class FeatureFlagsMapper : ResponseMapper<FeatureFlagsResponse, FeatureFlagDefinitions>
{
  public override Task<FeatureFlagsResponse> FromEntityAsync(FeatureFlagDefinitions definitions, CancellationToken ct)
  {
    var response = new FeatureFlagsResponse
    {
      FeatureManagement = new FeatureManagementSection
      {
        FeatureFlags = definitions.Flags.Select(f => new FeatureFlagEntry
        {
          Id = f.Id,
          Enabled = f.Enabled,
        }).ToList(),
      },
    };

    return Task.FromResult(response);
  }
}
