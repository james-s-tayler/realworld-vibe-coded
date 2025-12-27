namespace Server.Web.Identity.Login;

/// <summary>
/// Response model for bearer token login
/// </summary>
public class LoginResponse
{
  public string TokenType { get; set; } = "Bearer";

  public string AccessToken { get; set; } = default!;

  public int ExpiresIn { get; set; }

  public string RefreshToken { get; set; } = default!;
}
