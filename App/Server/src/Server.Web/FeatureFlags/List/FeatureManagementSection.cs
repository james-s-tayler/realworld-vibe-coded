using System.Text.Json.Serialization;

namespace Server.Web.FeatureFlags.List;

public class FeatureManagementSection
{
  [JsonPropertyName("feature_flags")]
  public List<FeatureFlagEntry> FeatureFlags { get; set; } = [];
}
