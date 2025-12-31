using Finbuckle.MultiTenant.AspNetCore.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Server.Core.IdentityAggregate;
using Server.Core.TenantInfoAggregate;
using Server.SharedKernel.Identity;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Persistence;

namespace Server.UseCases.Identity.Login;

public class LoginHandler : IQueryHandler<LoginCommand, LoginResult>
{
  private readonly IUserEmailChecker _userEmailChecker;
  private readonly IRepository<TenantInfo> _tenantRepository;
  private readonly IOptionsMonitor<BearerTokenOptions> _bearerTokenOptions;
  private readonly TimeProvider _timeProvider;
  private readonly ILogger<LoginHandler> _logger;
  private readonly IHttpContextAccessor _httpContextAccessor;

  public LoginHandler(
    IUserEmailChecker userEmailChecker,
    IRepository<TenantInfo> tenantRepository,
    IOptionsMonitor<BearerTokenOptions> bearerTokenOptions,
    TimeProvider timeProvider,
    ILogger<LoginHandler> logger,
    IHttpContextAccessor httpContextAccessor)
  {
    _userEmailChecker = userEmailChecker;
    _tenantRepository = tenantRepository;
    _bearerTokenOptions = bearerTokenOptions;
    _timeProvider = timeProvider;
    _logger = logger;
    _httpContextAccessor = httpContextAccessor;
  }

  public async Task<Result<LoginResult>> Handle(LoginCommand request, CancellationToken cancellationToken)
  {
    _logger.LogInformation("User {Email} attempting to log in", request.Email);

    // Find user by email across ALL tenants using IUserEmailChecker
    // This bypasses Finbuckle's query filters, which is necessary because the ClaimsStrategy
    // requires the user to be authenticated first before the tenant context can be resolved
    var user = await _userEmailChecker.FindByEmailAsync<ApplicationUser>(request.Email, cancellationToken);

    if (user == null)
    {
      _logger.LogWarning("Login failed for {Email}: User not found", request.Email);
      return Result<LoginResult>.Unauthorized(new ErrorDetail("email", "Invalid email or password."));
    }

    // Set tenant context before calling any UserManager/SignInManager methods
    // This is required so that EF Core queries for user claims work correctly
    var tenantId = _userEmailChecker.GetTenantId(user);
    var tenantInfo = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken);
    if (tenantInfo == null)
    {
      _logger.LogError("Tenant {TenantId} not found for user {Email}", tenantId, request.Email);
      return Result<LoginResult>.CriticalError(new ErrorDetail("Tenant not found"));
    }

    _httpContextAccessor.HttpContext!.SetTenantInfo(tenantInfo, resetServiceProviderScope: true);

    // Re-resolve UserManager and SignInManager after setting tenant context
    var userManager = _httpContextAccessor.HttpContext!.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
    var signInManager = _httpContextAccessor.HttpContext!.RequestServices.GetRequiredService<SignInManager<ApplicationUser>>();

    // Check lockout status directly from the user entity (already loaded)
    if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow)
    {
      _logger.LogWarning("Login failed for {Email}: Account is locked out", request.Email);
      return Result<LoginResult>.Unauthorized(new ErrorDetail("Account is locked out."));
    }

    var isPasswordValid = await userManager.CheckPasswordAsync(user, request.Password);
    if (!isPasswordValid)
    {
      // Handle access failed count directly via IUserEmailChecker instead of UserManager
      // to avoid entity tracking conflicts with the already-loaded user entity
      await _userEmailChecker.IncrementAccessFailedCountAsync(user, cancellationToken);
      _logger.LogWarning("Login failed for {Email}: Invalid password", request.Email);
      return Result<LoginResult>.Unauthorized(new ErrorDetail("Invalid email or password."));
    }

    // Check two-factor status directly from the user entity
    if (user.TwoFactorEnabled)
    {
      _logger.LogWarning("Login failed for {Email}: Two-factor authentication is required", request.Email);
      return Result<LoginResult>.Unauthorized(new ErrorDetail("Two-factor authentication is required."));
    }

    // Reset access failed count directly via IUserEmailChecker
    if (user.AccessFailedCount > 0)
    {
      await _userEmailChecker.ResetAccessFailedCountAsync(user, cancellationToken);
    }

    var useCookieScheme = request.UseCookies || request.UseSessionCookies;
    if (useCookieScheme)
    {
      var isPersistent = request.UseCookies && !request.UseSessionCookies;
      var principal = await signInManager.CreateUserPrincipalAsync(user);

      _logger.LogInformation("User {Email} logged in with cookies", request.Email);

      return Result<LoginResult>.Success(new LoginResult(
        AccessToken: string.Empty,
        ExpiresIn: 0,
        RefreshToken: string.Empty,
        Principal: principal,
        IsPersistent: isPersistent,
        RequiresCookieAuth: true));
    }

    var userPrincipal = await signInManager.CreateUserPrincipalAsync(user);
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
