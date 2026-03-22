namespace Server.Web.DevOnly.Endpoints.FeatureFlag;

public class CheckFeatureFlagResponse
{
  public string FeatureName { get; set; } = string.Empty;

  public bool IsEnabled { get; set; }
}
