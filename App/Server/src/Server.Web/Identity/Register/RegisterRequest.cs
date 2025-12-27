namespace Server.Web.Identity.Register;

/// <summary>
/// Request model for user registration
/// </summary>
public class RegisterRequest
{
  public string Email { get; set; } = default!;

  public string Password { get; set; } = default!;
}
