using Finbuckle.MultiTenant.Abstractions;
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
  private readonly IMultiTenantStore<TenantInfo> _tenantStore;
  private readonly IMultiTenantContextSetter _contextSetter;
  private readonly ITenantAssigner _tenantAssigner;
  private readonly ILogger<RegisterHandler> _logger;

  public RegisterHandler(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole<Guid>> roleManager,
    IMultiTenantStore<TenantInfo> tenantStore,
    IMultiTenantContextSetter contextSetter,
    ITenantAssigner tenantAssigner,
    ILogger<RegisterHandler> logger)
  {
    _userManager = userManager;
    _roleManager = roleManager;
    _tenantStore = tenantStore;
    _contextSetter = contextSetter;
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

    // Create TenantInfo for the new user's organization
    var tenantId = Guid.NewGuid().ToString();
    var tenantIdentifier = tenantId; // Using tenant ID as the identifier for simplicity

    _logger.LogInformation("Creating tenant with ID {TenantId}", tenantId);
    var tenant = new TenantInfo(tenantId, tenantIdentifier, "New Company");

    var added = await _tenantStore.AddAsync(tenant);
    if (!added)
    {
      _logger.LogError("Failed to add tenant to store");
      return Result<Unit>.Error(new ErrorDetail("tenant", "Failed to create tenant"));
    }

    // Set tenant context using Finbuckle's standard IMultiTenantContextSetter
    _contextSetter.MultiTenantContext = new MultiTenantContext<TenantInfo>
    {
      TenantInfo = tenant,
      StoreInfo = new StoreInfo<TenantInfo> { Store = _tenantStore },
    };

    // Ensure Owner role exists (now that tenant context is set)
    const string ownerRoleName = "Owner";
    if (!await _roleManager.RoleExistsAsync(ownerRoleName))
    {
      _logger.LogInformation("Creating Owner role");
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
    await _tenantAssigner.SetTenantIdAsync(user.Id, tenantId, cancellationToken);
    _logger.LogInformation("Set TenantId {TenantId} for user {Email}", tenantId, request.Email);

    // Assign Owner role
    _logger.LogInformation("Assigning Owner role to user {Email}", request.Email);
    var roleAssignResult = await _userManager.AddToRoleAsync(user, ownerRoleName);

    if (!roleAssignResult.Succeeded)
    {
      _logger.LogError("Failed to assign Owner role to user: {Errors}", string.Join(", ", roleAssignResult.Errors.Select(e => e.Description)));
      return Result<Unit>.Error(new ErrorDetail("role", "Failed to assign Owner role"));
    }

    _logger.LogInformation("User {Email} registered successfully with tenant {TenantId}", request.Email, tenantId);
    return Result<Unit>.Success(Unit.Value);
  }
}
