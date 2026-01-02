using System.Security.Claims;
using Destructurama.Attributed;

namespace Server.UseCases.Identity.Login;

public class LoginResult
{
  [NotLogged]
  public string AccessToken { get; init; } = default!;

  public int ExpiresIn { get; init; }

  [NotLogged]
  public string RefreshToken { get; init; } = default!;

  public ClaimsPrincipal? Principal { get; init; }

  public bool IsPersistent { get; init; }

  public bool RequiresCookieAuth { get; init; }
}
