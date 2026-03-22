namespace Server.SharedKernel.Interfaces;

public interface IFeatureFlagService
{
  Task<bool> IsEnabledAsync(string featureName);
}
