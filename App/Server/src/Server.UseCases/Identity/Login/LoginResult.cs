using System.Security.Claims;

namespace Server.UseCases.Identity.Login;

public record LoginResult(
  string AccessToken,
  int ExpiresIn,
  string RefreshToken,
  ClaimsPrincipal? Principal,
  bool IsPersistent,
  bool RequiresCookieAuth);
