using Server.SharedKernel.Interfaces;
using Server.SharedKernel.MediatR;
using Server.Web.DevOnly.Endpoints.FeatureFlag;

namespace Server.Web.DevOnly.UseCases.FeatureFlag;

#pragma warning disable SRV015 // DevOnly test endpoint
public class CheckFeatureFlagHandler(IFeatureFlagService featureFlagService) : IQueryHandler<CheckFeatureFlagQuery, CheckFeatureFlagResponse>
{
  public async Task<Result<CheckFeatureFlagResponse>> Handle(CheckFeatureFlagQuery request, CancellationToken cancellationToken)
  {
    var isEnabled = await featureFlagService.IsEnabledAsync(request.FeatureName);
    return Result<CheckFeatureFlagResponse>.Success(new CheckFeatureFlagResponse
    {
      FeatureName = request.FeatureName,
      IsEnabled = isEnabled,
    });
  }
}
#pragma warning restore SRV015
