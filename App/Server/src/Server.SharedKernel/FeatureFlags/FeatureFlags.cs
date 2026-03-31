namespace Server.SharedKernel.FeatureFlags;

public static class FeatureFlags
{
  public const string DashboardBanner = "DashboardBanner";
  public const string DisabledFeature = "DisabledFeature";
  public const string SampleFeature = "SampleFeature";

  public static readonly string[] ClientVisible = [DashboardBanner];
}
