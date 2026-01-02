using Destructurama.Attributed;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Identity.Login;

public class LoginCommand : IQuery<LoginResult>
{
  public required string Email { get; set; }

  [NotLogged]
  public required string Password { get; set; }

  public bool UseCookies { get; set; }

  public bool UseSessionCookies { get; set; }
}
