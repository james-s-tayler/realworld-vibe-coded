using Microsoft.Extensions.Logging;
using Server.SharedKernel.Interfaces;
using Server.SharedKernel.MediatR;
using FeatureFlagConstants = Server.SharedKernel.FeatureFlags.FeatureFlags;

namespace Server.UseCases.FeatureFlags.List;

public class ListFeatureFlagsHandler(
  IFeatureFlagService featureFlagService,
  ILogger<ListFeatureFlagsHandler> logger) : IQueryHandler<ListFeatureFlagsQuery, FeatureFlagDefinitions>
{
  public async Task<Result<FeatureFlagDefinitions>> Handle(ListFeatureFlagsQuery request, CancellationToken cancellationToken)
  {
    var flags = new List<FeatureFlagItem>();

    foreach (var flagName in FeatureFlagConstants.ClientVisible)
    {
      var enabled = await featureFlagService.IsEnabledAsync(flagName);
      flags.Add(new FeatureFlagItem(flagName, enabled));
    }

    logger.LogInformation("Retrieved {Count} client-visible feature flags", flags.Count);

    return Result<FeatureFlagDefinitions>.Success(new FeatureFlagDefinitions(flags));
  }
}
