using Microsoft.Extensions.Configuration;

namespace Server.Web.DevOnly.Configuration;

public sealed class FeatureFlagOverrideProvider : ConfigurationProvider
{
  public void SetOverride(string featureName, bool enabled, IConfiguration rootConfig)
  {
    var index = FindFlagIndex(featureName, rootConfig);
    if (index < 0)
    {
      return;
    }

    Data[$"feature_management:feature_flags:{index}:enabled"] = enabled ? "True" : "False";
    OnReload();
  }

  public void ClearOverride(string featureName, IConfiguration rootConfig)
  {
    var index = FindFlagIndex(featureName, rootConfig);
    if (index < 0)
    {
      return;
    }

    Data.Remove($"feature_management:feature_flags:{index}:enabled");
    OnReload();
  }

  public void ClearAllOverrides()
  {
    Data.Clear();
    OnReload();
  }

  private static int FindFlagIndex(string featureName, IConfiguration rootConfig)
  {
    var flagsSection = rootConfig.GetSection("feature_management:feature_flags");
    foreach (var child in flagsSection.GetChildren())
    {
      if (string.Equals(child["id"], featureName, StringComparison.OrdinalIgnoreCase))
      {
        return int.Parse(child.Key);
      }
    }

    return -1;
  }
}
