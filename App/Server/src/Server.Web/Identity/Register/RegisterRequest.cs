using Destructurama.Attributed;

namespace Server.Web.Identity.Register;

public class RegisterRequest
{
  public string Email { get; set; } = default!;

  [NotLogged]
  public string Password { get; set; } = default!;
}
