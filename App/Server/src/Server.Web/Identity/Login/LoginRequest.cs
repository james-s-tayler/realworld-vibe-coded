namespace Server.Web.Identity.Login;

/// <summary>
/// Request model for user login
/// </summary>
public class LoginRequest
{
  public string Email { get; set; } = default!;

  public string Password { get; set; } = default!;
}
