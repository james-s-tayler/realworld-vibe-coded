using System.Text.Json.Serialization;

namespace E2eTests;

/// <summary>
/// Represents a user from the list users API response.
/// </summary>
public class ListedUser
{
  [JsonPropertyName("id")]
  public string Id { get; set; } = string.Empty;

  [JsonPropertyName("email")]
  public string Email { get; set; } = string.Empty;

  [JsonPropertyName("username")]
  public string Username { get; set; } = string.Empty;

  [JsonPropertyName("roles")]
  public List<string> Roles { get; set; } = [];

  [JsonPropertyName("isActive")]
  public bool IsActive { get; set; }
}
