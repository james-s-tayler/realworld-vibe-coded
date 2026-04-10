using System.Text.Json.Serialization;

namespace Server.Web.FeatureFlags.List;

public class FeatureFlagsResponse
{
  [JsonPropertyName("feature_management")]
  public FeatureManagementSection FeatureManagement { get; set; } = new();
}
