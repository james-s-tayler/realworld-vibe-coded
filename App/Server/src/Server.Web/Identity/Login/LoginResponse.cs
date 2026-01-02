using Destructurama.Attributed;

namespace Server.Web.Identity.Login;

public class LoginResponse
{
  public string TokenType { get; set; } = "Bearer";

  [NotLogged]
  public string AccessToken { get; set; } = default!;

  public int ExpiresIn { get; set; }

  [NotLogged]
  public string RefreshToken { get; set; } = default!;
}
