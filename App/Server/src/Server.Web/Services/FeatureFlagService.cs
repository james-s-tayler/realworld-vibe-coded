using Microsoft.FeatureManagement;
using Server.SharedKernel.Interfaces;

namespace Server.Web.Services;

public class FeatureFlagService(IFeatureManager featureManager) : IFeatureFlagService
{
  public Task<bool> IsEnabledAsync(string featureName) =>
    featureManager.IsEnabledAsync(featureName);
}
