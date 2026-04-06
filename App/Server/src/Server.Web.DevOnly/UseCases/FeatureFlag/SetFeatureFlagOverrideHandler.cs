using Microsoft.Extensions.Configuration;
using Server.SharedKernel.Interfaces;
using Server.Web.DevOnly.Endpoints.FeatureFlag;

namespace Server.Web.DevOnly.UseCases.FeatureFlag;

#pragma warning disable SRV015 // DevOnly test endpoint
#pragma warning disable PV014 // DevOnly - no repository mutation needed
public class SetFeatureFlagOverrideHandler(
  FeatureFlagOverrideProvider overrideProvider,
  IConfiguration configuration,
  IFeatureFlagService featureFlagService) : Server.SharedKernel.MediatR.ICommandHandler<SetFeatureFlagOverrideCommand, CheckFeatureFlagResponse>
{
  public async Task<Result<CheckFeatureFlagResponse>> Handle(SetFeatureFlagOverrideCommand request, CancellationToken cancellationToken)
  {
    overrideProvider.SetOverride(request.FeatureName, request.Enabled, configuration);

    var isEnabled = await featureFlagService.IsEnabledAsync(request.FeatureName);

    return Result<CheckFeatureFlagResponse>.Success(new CheckFeatureFlagResponse
    {
      FeatureName = request.FeatureName,
      IsEnabled = isEnabled,
    });
  }
}
#pragma warning restore PV014
#pragma warning restore SRV015
