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
  private readonly SignInManager<ApplicationUser> _signInManager;
  private readonly IOptionsMonitor<BearerTokenOptions> _bearerTokenOptions;
  private readonly TimeProvider _timeProvider;
  private readonly ILogger<LoginHandler> _logger;

  public LoginHandler(
    SignInManager<ApplicationUser> signInManager,
    IOptionsMonitor<BearerTokenOptions> bearerTokenOptions,
    TimeProvider timeProvider,
    ILogger<LoginHandler> logger)
  {
    _signInManager = signInManager;
    _bearerTokenOptions = bearerTokenOptions;
    _timeProvider = timeProvider;
    _logger = logger;
  }

  public async Task<Result<LoginResult>> Handle(LoginCommand request, CancellationToken cancellationToken)
  {
    _logger.LogInformation("User {Email} attempting to log in", request.Email);

    var useCookieScheme = request.UseCookies || request.UseSessionCookies;
    var isPersistent = request.UseCookies && !request.UseSessionCookies;

    _signInManager.AuthenticationScheme = useCookieScheme
      ? IdentityConstants.ApplicationScheme
      : IdentityConstants.BearerScheme;

    var result = await _signInManager.PasswordSignInAsync(
      request.Email,
      request.Password,
      isPersistent,
      lockoutOnFailure: true);

    if (!result.Succeeded)
    {
      var errorMessage = result switch
      {
        { IsLockedOut: true } => "Account is locked out.",
        { RequiresTwoFactor: true } => "Two-factor authentication is required.",
        { IsNotAllowed: true } => "Account is not allowed to sign in.",
        _ => "Invalid email or password.",
      };

      _logger.LogWarning("Login failed for {Email}: {Reason}", request.Email, errorMessage);
      return Result<LoginResult>.Unauthorized(new ErrorDetail(errorMessage));
    }

    if (useCookieScheme)
    {
      _logger.LogInformation("User {Email} logged in with cookies", request.Email);
      return Result<LoginResult>.NoContent();
    }

    var user = await _signInManager.UserManager.FindByEmailAsync(request.Email);
    if (user == null)
    {
      _logger.LogError("User {Email} not found after successful sign-in", request.Email);
      return Result<LoginResult>.Unauthorized(new ErrorDetail("User not found."));
    }

    var principal = await _signInManager.CreateUserPrincipalAsync(user);
    var bearerOptions = _bearerTokenOptions.Get(IdentityConstants.BearerScheme);

    var accessTokenExpiration = _timeProvider.GetUtcNow() + bearerOptions.BearerTokenExpiration;
    var accessToken = bearerOptions.BearerTokenProtector.Protect(CreateBearerTicket(principal, accessTokenExpiration));

    var refreshTokenExpiration = _timeProvider.GetUtcNow() + bearerOptions.RefreshTokenExpiration;
    var refreshToken = bearerOptions.RefreshTokenProtector.Protect(CreateBearerTicket(principal, refreshTokenExpiration));

    var loginResult = new LoginResult(
      accessToken,
      (int)bearerOptions.BearerTokenExpiration.TotalSeconds,
      refreshToken);

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
