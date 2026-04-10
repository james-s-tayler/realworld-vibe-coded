namespace Server.Web.DevOnly.Endpoints.FeatureFlag;

public class SetFeatureFlagOverrideRequest
{
  [RouteParam]
  public string FeatureName { get; set; } = string.Empty;

  public bool Enabled { get; set; }
}
