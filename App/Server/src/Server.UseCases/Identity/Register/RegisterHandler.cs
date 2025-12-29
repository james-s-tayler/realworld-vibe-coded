using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Server.Core.IdentityAggregate;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Identity.Register;

// this class shouldn't return ApplicationUser as the payload since that includes the password hash!
#pragma warning disable SRV015
public class RegisterHandler : ICommandHandler<RegisterCommand, Unit>
{
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly RoleManager<IdentityRole<Guid>> _roleManager;
  private readonly ITenantAssigner _tenantAssigner;
  private readonly ILogger<RegisterHandler> _logger;

  public RegisterHandler(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole<Guid>> roleManager,
    ITenantAssigner tenantAssigner,
    ILogger<RegisterHandler> logger)
  {
    _userManager = userManager;
    _roleManager = roleManager;
    _tenantAssigner = tenantAssigner;
    _logger = logger;
  }

  // PV014: UserManager.CreateAsync is a mutation operation, but the analyzer doesn't recognize it
  // as a repository method. Suppressing this false positive.
#pragma warning disable PV014

  public async Task<Result<Unit>> Handle(RegisterCommand request, CancellationToken cancellationToken)

#pragma warning restore PV014

  {
    _logger.LogInformation("Registering new user with email {Email}", request.Email);

    // Tenant context should already be set by the endpoint before this handler is called
    // This ensures RoleManager and UserManager see the correct tenant context

    // Ensure Owner role exists (tenant context is already set at endpoint level)
    const string ownerRoleName = "Owner";
    if (!await _roleManager.RoleExistsAsync(ownerRoleName))
    {
      _logger.LogInformation("Creating Owner role for tenant {TenantId}", request.TenantId);
      var roleResult = await _roleManager.CreateAsync(new IdentityRole<Guid>(ownerRoleName));
      if (!roleResult.Succeeded)
      {
        _logger.LogError("Failed to create Owner role: {Errors}", string.Join(", ", roleResult.Errors.Select(e => e.Description)));
        return Result<Unit>.Error(new ErrorDetail("role", "Failed to create Owner role"));
      }
    }

    // Create user
    var user = new ApplicationUser
    {
      UserName = request.Email,
      Email = request.Email,
    };

    var result = await _userManager.CreateAsync(user, request.Password);

    if (!result.Succeeded)
    {
      var errorDetails = result.Errors.Select(e => new ErrorDetail("email", e.Description)).ToArray();
      _logger.LogWarning("User registration failed for {Email}: {Errors}", request.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
      return Result<Unit>.Invalid(errorDetails);
    }

    // Set TenantId shadow property using TenantAssigner (which handles TenantMismatchMode)
    await _tenantAssigner.SetTenantIdAsync(user.Id, request.TenantId, cancellationToken);
    _logger.LogInformation("Set TenantId {TenantId} for user {Email}", request.TenantId, request.Email);

    // Assign Owner role
    _logger.LogInformation("Assigning Owner role to user {Email}", request.Email);
    var roleAssignResult = await _userManager.AddToRoleAsync(user, ownerRoleName);

    if (!roleAssignResult.Succeeded)
    {
      _logger.LogError("Failed to assign Owner role to user: {Errors}", string.Join(", ", roleAssignResult.Errors.Select(e => e.Description)));
      return Result<Unit>.Error(new ErrorDetail("role", "Failed to assign Owner role"));
    }

    _logger.LogInformation("User {Email} registered successfully with tenant {TenantId}", request.Email, request.TenantId);
    return Result<Unit>.Success(Unit.Value);
  }
}
