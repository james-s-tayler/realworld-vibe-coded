namespace E2eTests;

/// <summary>
/// Represents a user created via the API for testing.
/// </summary>
public class CreatedUser
{
  public string Token { get; set; } = string.Empty;

  public string Username { get; set; } = string.Empty;

  public string Email { get; set; } = string.Empty;

  public string Password { get; set; } = string.Empty;
}
