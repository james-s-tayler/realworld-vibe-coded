namespace Server.Web.DevOnly.Endpoints.FeatureFlag;

public class CheckFeatureFlagRequest
{
  [RouteParam]
  public string FeatureName { get; set; } = string.Empty;
}
