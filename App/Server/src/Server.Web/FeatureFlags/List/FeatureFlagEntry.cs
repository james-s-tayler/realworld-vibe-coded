using System.Text.Json.Serialization;

namespace Server.Web.FeatureFlags.List;

public class FeatureFlagEntry
{
  [JsonPropertyName("id")]
  public string Id { get; set; } = string.Empty;

  [JsonPropertyName("enabled")]
  public bool Enabled { get; set; }
}
