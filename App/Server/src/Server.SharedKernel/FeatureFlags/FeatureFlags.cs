namespace Server.SharedKernel.FeatureFlags;

public static class FeatureFlags
{
  public const string DisabledFeature = "DisabledFeature";
  public const string SampleFeature = "SampleFeature";
  public const string TargetingTestFlag = "TargetingTestFlag";

  public static readonly string[] ClientVisible = [];
}
