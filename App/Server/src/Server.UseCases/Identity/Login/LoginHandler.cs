using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Server.Core.IdentityAggregate;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Identity.Login;

public class LoginHandler : IQueryHandler<LoginCommand, LoginResult>
{
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly SignInManager<ApplicationUser> _signInManager;
  private readonly IOptionsMonitor<BearerTokenOptions> _bearerTokenOptions;
  private readonly TimeProvider _timeProvider;
  private readonly ILogger<LoginHandler> _logger;

  public LoginHandler(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IOptionsMonitor<BearerTokenOptions> bearerTokenOptions,
    TimeProvider timeProvider,
    ILogger<LoginHandler> logger)
  {
    _userManager = userManager;
    _signInManager = signInManager;
    _bearerTokenOptions = bearerTokenOptions;
    _timeProvider = timeProvider;
    _logger = logger;
  }

  public async Task<Result<LoginResult>> Handle(LoginCommand request, CancellationToken cancellationToken)
  {
    _logger.LogInformation("User {Email} attempting to log in", request.Email);

    var user = await _userManager.FindByEmailAsync(request.Email);
    if (user == null)
    {
      _logger.LogWarning("Login failed for {Email}: User not found", request.Email);
      return Result<LoginResult>.Unauthorized(new ErrorDetail("email", "Invalid email or password."));
    }

    if (await _userManager.IsLockedOutAsync(user))
    {
      _logger.LogWarning("Login failed for {Email}: Account is locked out", request.Email);
      return Result<LoginResult>.Unauthorized(new ErrorDetail("Account is locked out."));
    }

    var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
    if (!isPasswordValid)
    {
      await _userManager.AccessFailedAsync(user);
      _logger.LogWarning("Login failed for {Email}: Invalid password", request.Email);
      return Result<LoginResult>.Unauthorized(new ErrorDetail("Invalid email or password."));
    }

    if (await _userManager.GetTwoFactorEnabledAsync(user))
    {
      _logger.LogWarning("Login failed for {Email}: Two-factor authentication is required", request.Email);
      return Result<LoginResult>.Unauthorized(new ErrorDetail("Two-factor authentication is required."));
    }

    await _userManager.ResetAccessFailedCountAsync(user);

    var useCookieScheme = request.UseCookies || request.UseSessionCookies;
    if (useCookieScheme)
    {
      var isPersistent = request.UseCookies && !request.UseSessionCookies;
      var principal = await _signInManager.CreateUserPrincipalAsync(user);

      _logger.LogInformation("User {Email} logged in with cookies", request.Email);

      return Result<LoginResult>.Success(new LoginResult(
        AccessToken: string.Empty,
        ExpiresIn: 0,
        RefreshToken: string.Empty,
        Principal: principal,
        IsPersistent: isPersistent,
        RequiresCookieAuth: true));
    }

    var userPrincipal = await _signInManager.CreateUserPrincipalAsync(user);
    var bearerOptions = _bearerTokenOptions.Get(IdentityConstants.BearerScheme);

    var accessTokenExpiration = _timeProvider.GetUtcNow() + bearerOptions.BearerTokenExpiration;
    var accessToken = bearerOptions.BearerTokenProtector.Protect(CreateBearerTicket(userPrincipal, accessTokenExpiration));

    var refreshTokenExpiration = _timeProvider.GetUtcNow() + bearerOptions.RefreshTokenExpiration;
    var refreshToken = bearerOptions.RefreshTokenProtector.Protect(CreateBearerTicket(userPrincipal, refreshTokenExpiration));

    var loginResult = new LoginResult(
      AccessToken: accessToken,
      ExpiresIn: (int)bearerOptions.BearerTokenExpiration.TotalSeconds,
      RefreshToken: refreshToken,
      Principal: null,
      IsPersistent: false,
      RequiresCookieAuth: false);

    _logger.LogInformation("User {Email} logged in with bearer token", request.Email);

    return Result<LoginResult>.Success(loginResult);
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
