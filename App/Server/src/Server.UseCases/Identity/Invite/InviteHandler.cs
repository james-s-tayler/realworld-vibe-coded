using System.Security.Claims;
using Finbuckle.MultiTenant.AspNetCore.Extensions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Core.IdentityAggregate;
using Server.SharedKernel.Identity;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Identity.Invite;

// this class shouldn't return ApplicationUser as the payload since that includes the password hash!
#pragma warning disable SRV015
public class InviteHandler : ICommandHandler<InviteCommand, Unit>
{
  private readonly IUserEmailChecker _userEmailChecker;
  private readonly ILogger<InviteHandler> _logger;
  private readonly IHttpContextAccessor _httpContextAccessor;

  public InviteHandler(
    IUserEmailChecker userEmailChecker,
    ILogger<InviteHandler> logger,
    IHttpContextAccessor httpContextAccessor)
  {
    _userEmailChecker = userEmailChecker;
    _logger = logger;
    _httpContextAccessor = httpContextAccessor;
  }

  // PV014: UserManager.CreateAsync is a mutation operation, but the analyzer doesn't recognize it
  // as a repository method. Suppressing this false positive.
#pragma warning disable PV014

  public async Task<Result<Unit>> Handle(InviteCommand request, CancellationToken cancellationToken)
#pragma warning restore PV014
  {
    // Tenant context is already resolved from the authenticated user's __tenant__ claim
    // via Finbuckle's ClaimsStrategy - no need to manually set it
    var tenantInfo = _httpContextAccessor.HttpContext!.GetTenantInfo<Server.Core.TenantInfoAggregate.TenantInfo>();
    if (tenantInfo == null)
    {
      _logger.LogError("User invitation failed: No tenant context found");
      return Result<Unit>.Invalid(new ErrorDetail("tenant", "No tenant context found"));
    }

    var tenantId = tenantInfo.Id!;
    _logger.LogInformation("Inviting user to tenant {TenantId}", tenantId);

    // Check for duplicate email across ALL tenants
    var emailExists = await _userEmailChecker.EmailExistsAsync(request.Email, cancellationToken);
    if (emailExists)
    {
      _logger.LogWarning("User invitation failed for {Email}: Duplicate email", request.Email);
      return Result<Unit>.Invalid(new ErrorDetail("email", "A user has already been registered with that email"));
    }

    _logger.LogInformation("Creating new user with email {Email} in tenant {TenantId}", request.Email, tenantId);

    var user = new ApplicationUser
    {
      UserName = request.Email,
      Email = request.Email,
    };

    // UserManager is tenant-scoped via the already-resolved tenant context
    var userManager = _httpContextAccessor.HttpContext!.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();

    var result = await userManager.CreateAsync(user, request.Password);

    if (!result.Succeeded)
    {
      var errorDetails = result.Errors.Select(e => new ErrorDetail("email", e.Description)).ToArray();
      _logger.LogWarning("User invitation failed for {Email}: {Errors}", request.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
      return Result<Unit>.Invalid(errorDetails);
    }

    _logger.LogInformation("Created new user with email {Email}", request.Email);

    _logger.LogInformation("Assigning AUTHOR role to invited user");

    var authorRoleResult = await userManager.AddToRoleAsync(user, ApplicationRoles.Author);
    if (!authorRoleResult.Succeeded)
    {
      var errorDetails = authorRoleResult.Errors.Select(e => new ErrorDetail("role", e.Description)).ToArray();
      _logger.LogError("Failed to add AUTHOR role for user {Email}", request.Email);
      return Result<Unit>.Invalid(errorDetails);
    }

    _logger.LogInformation("Assigned AUTHOR role to invited user");

    // Add tenant claim
    var tenantClaim = new Claim("__tenant__", tenantId);
    _logger.LogInformation("Adding claim to invited user __tenant__: {@Claim}", tenantClaim);

    var claimResult = await userManager.AddClaimAsync(user, tenantClaim);
    _logger.LogInformation("Added claim to invited user __tenant__: {@Claim}", tenantClaim);

    if (!claimResult.Succeeded)
    {
      var errorDetails = claimResult.Errors.Select(e => new ErrorDetail("tenantId", e.Description)).ToArray();
      _logger.LogError("Failed to add tenant claim for user {Email}", request.Email);
      return Result<Unit>.Invalid(errorDetails);
    }

    _logger.LogInformation("User {Email} invited successfully to tenant {TenantId}", request.Email, tenantId);

    return Result<Unit>.NoContent();
  }
}
