using System.Text.Json.Serialization;

namespace Server.Web.Config.Get;

public class ConfigResponse
{
  [JsonPropertyName("feature_flag_refresh_interval_seconds")]
  public int FeatureFlagRefreshIntervalSeconds { get; set; }
}
