using Destructurama.Attributed;

namespace Server.Web.Identity.Login;

public class LoginRequest
{
  public string Email { get; set; } = default!;

  [NotLogged]
  public string Password { get; set; } = default!;
}
