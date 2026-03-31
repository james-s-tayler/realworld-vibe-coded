namespace Server.SharedKernel.FeatureFlags;

public class FeatureFlagSettings
{
  public const string SectionName = "FeatureFlagSettings";

  public int RefreshIntervalSeconds { get; set; } = 30;
}
