using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Server.Core.IdentityAggregate;

namespace Server.Web.Identity.Login;

/// <summary>
/// Login endpoint that authenticates users via cookie or bearer token
/// </summary>
public class Login(
  SignInManager<ApplicationUser> signInManager,
  IOptionsMonitor<BearerTokenOptions> bearerTokenOptions,
  TimeProvider timeProvider) : Endpoint<LoginRequest>
{
  public override void Configure()
  {
    Post("/api/identity/login");
    AllowAnonymous();
  }

  public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
  {
    // Check query parameters to determine authentication scheme
    var useCookies = HttpContext.Request.Query["useCookies"].ToString().Equals("true", StringComparison.OrdinalIgnoreCase);
    var useSessionCookies = HttpContext.Request.Query["useSessionCookies"].ToString().Equals("true", StringComparison.OrdinalIgnoreCase);

    var useCookieScheme = useCookies || useSessionCookies;
    var isPersistent = useCookies && !useSessionCookies;

    // Set authentication scheme
    signInManager.AuthenticationScheme = useCookieScheme
      ? IdentityConstants.ApplicationScheme
      : IdentityConstants.BearerScheme;

    var result = await signInManager.PasswordSignInAsync(
      req.Email,
      req.Password,
      isPersistent,
      lockoutOnFailure: true);

    if (!result.Succeeded)
    {
      ThrowError(result.ToString(), StatusCodes.Status401Unauthorized);
      return;
    }

    // If using cookies, sign in already set the cookie, return empty response
    if (useCookieScheme)
    {
      HttpContext.Response.StatusCode = StatusCodes.Status200OK;
      return;
    }

    // If using bearer tokens, generate and return access token
    var user = await signInManager.UserManager.FindByEmailAsync(req.Email);
    if (user == null)
    {
      ThrowError("User not found.", StatusCodes.Status401Unauthorized);
      return;
    }

    var principal = await signInManager.CreateUserPrincipalAsync(user);
    var bearerOptions = bearerTokenOptions.Get(IdentityConstants.BearerScheme);

    // Generate access token
    var accessTokenExpiration = timeProvider.GetUtcNow() + bearerOptions.BearerTokenExpiration;
    var accessToken = bearerOptions.BearerTokenProtector.Protect(CreateBearerTicket(principal, accessTokenExpiration));

    // Generate refresh token
    var refreshTokenExpiration = timeProvider.GetUtcNow() + bearerOptions.RefreshTokenExpiration;
    var refreshToken = bearerOptions.RefreshTokenProtector.Protect(CreateBearerTicket(principal, refreshTokenExpiration));

    var response = new LoginResponse
    {
      AccessToken = accessToken,
      ExpiresIn = (int)bearerOptions.BearerTokenExpiration.TotalSeconds,
      RefreshToken = refreshToken,
    };

    // Use ASP.NET Core's Results API to send JSON
    await Results.Json(response).ExecuteAsync(HttpContext);
  }

  private static AuthenticationTicket CreateBearerTicket(System.Security.Claims.ClaimsPrincipal principal, DateTimeOffset expiration)
  {
    var properties = new AuthenticationProperties
    {
      ExpiresUtc = expiration,
    };

    return new AuthenticationTicket(principal, properties, IdentityConstants.BearerScheme);
  }
}
