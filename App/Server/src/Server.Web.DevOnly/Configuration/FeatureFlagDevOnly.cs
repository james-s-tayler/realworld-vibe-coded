namespace Server.Web.DevOnly.Configuration;

public sealed class FeatureFlagDevOnly : SubGroup<DevOnly>
{
  public FeatureFlagDevOnly()
  {
    Configure("feature-flags", ep => { ep.AllowAnonymous(); });
  }
}
