using Destructurama.Attributed;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Identity.Login;

public class LoginCommand : IQuery<LoginResult>
{
  public required string Email { get; init; }

  [NotLogged]
  public required string Password { get; init; }

  public bool UseCookies { get; init; }

  public bool UseSessionCookies { get; init; }
}
