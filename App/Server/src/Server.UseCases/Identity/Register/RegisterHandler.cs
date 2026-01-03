using System.Security.Claims;
using Finbuckle.MultiTenant.AspNetCore.Extensions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Core.IdentityAggregate;
using Server.Core.TenantInfoAggregate;
using Server.SharedKernel.Identity;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Persistence;

namespace Server.UseCases.Identity.Register;

// this class shouldn't return ApplicationUser as the payload since that includes the password hash!
#pragma warning disable SRV015
public class RegisterHandler : ICommandHandler<RegisterCommand, Unit>
{
  private readonly IRepository<TenantInfo> _tenantRepository;
  private readonly IUserEmailChecker _userEmailChecker;
  private readonly ILogger<RegisterHandler> _logger;
  private readonly IHttpContextAccessor _httpContextAccessor;

  public RegisterHandler(
    IRepository<TenantInfo> tenantRepository,
    IUserEmailChecker userEmailChecker,
    ILogger<RegisterHandler> logger,
    IHttpContextAccessor httpContextAccessor)
  {
    _tenantRepository = tenantRepository;
    _userEmailChecker = userEmailChecker;
    _logger = logger;
    _httpContextAccessor = httpContextAccessor;
  }

  // PV014: UserManager.CreateAsync is a mutation operation, but the analyzer doesn't recognize it
  // as a repository method. Suppressing this false positive.
#pragma warning disable PV014

  public async Task<Result<Unit>> Handle(RegisterCommand request, CancellationToken cancellationToken)
#pragma warning restore PV014
  {
    // Check for duplicate email across ALL tenants before creating tenant or user
    var emailExists = await _userEmailChecker.EmailExistsAsync(request.Email, cancellationToken);
    if (emailExists)
    {
      _logger.LogWarning("User registration failed for {Email}: Duplicate email", request.Email);
      return Result<Unit>.Invalid(new ErrorDetail("email", "A user has already been registered with that email"));
    }

    var tenantId = Guid.NewGuid().ToString();

    var tenantInfo = new TenantInfo(
      id: tenantId,
      identifier: tenantId,
      name: "My Company");

    _logger.LogInformation("Manually setting tenant context: {@Tenant}", tenantInfo);

    _httpContextAccessor.HttpContext!.SetTenantInfo(tenantInfo, resetServiceProviderScope: true);

    _logger.LogInformation("Manually set tenant context: {@Tenant}", tenantInfo);

    _logger.LogInformation("Registering new tenant: {@Tenant}", tenantInfo);

    await _tenantRepository.AddAsync(tenantInfo, cancellationToken);
    await _tenantRepository.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("Registered new tenant");

    _logger.LogInformation("Registering new user with email {Email}", request.Email);

    var user = new ApplicationUser
    {
      UserName = request.Email,
      Email = request.Email,
    };

    // the service needs to be re-resolved here because of the call to SetTenantInfo -
    // if we try constructor injection, we'll get a copy of the dependency without the tenant info and the global query filters will fail
    var userManager = _httpContextAccessor.HttpContext!.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = _httpContextAccessor.HttpContext!.RequestServices.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

    var result = await userManager.CreateAsync(user, request.Password);

    if (!result.Succeeded)
    {
      var errorDetails = result.Errors.Select(e => new ErrorDetail("email", e.Description)).ToArray();
      _logger.LogWarning("User registration failed for {Email}: {Errors}", request.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
      return Result<Unit>.Invalid(errorDetails);
    }

    _logger.LogInformation("Registered new user");

    var rolesToCreate = new[] { ApplicationRoles.Owner, ApplicationRoles.Admin, ApplicationRoles.Author, ApplicationRoles.Moderator };
    foreach (var roleName in rolesToCreate)
    {
      if (!await roleManager.RoleExistsAsync(roleName))
      {
        _logger.LogInformation("Creating role {RoleName}", roleName);
        var roleResult = await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
        if (!roleResult.Succeeded)
        {
          var errorDetails = roleResult.Errors.Select(e => new ErrorDetail("role", e.Description)).ToArray();
          _logger.LogError("Failed to create role {RoleName} for tenant {TenantId}", roleName, tenantId);
          return Result<Unit>.Invalid(errorDetails);
        }
      }
    }

    _logger.LogInformation("Assigning OWNER and ADMIN roles to new user");

    var ownerRoleResult = await userManager.AddToRoleAsync(user, ApplicationRoles.Owner);
    if (!ownerRoleResult.Succeeded)
    {
      var errorDetails = ownerRoleResult.Errors.Select(e => new ErrorDetail("role", e.Description)).ToArray();
      _logger.LogError("Failed to add OWNER role for user {Email}", request.Email);
      return Result<Unit>.Invalid(errorDetails);
    }

    var adminRoleResult = await userManager.AddToRoleAsync(user, ApplicationRoles.Admin);
    if (!adminRoleResult.Succeeded)
    {
      var errorDetails = adminRoleResult.Errors.Select(e => new ErrorDetail("role", e.Description)).ToArray();
      _logger.LogError("Failed to add ADMIN role for user {Email}", request.Email);
      return Result<Unit>.Invalid(errorDetails);
    }

    _logger.LogInformation("Assigned OWNER and ADMIN roles to new user");

    var tenantClaim = new Claim("__tenant__", tenantId);

    _logger.LogInformation("Adding claim to new user __tenant__: {@Claim}", tenantClaim);

    var claimResult = await userManager.AddClaimAsync(user, tenantClaim);

    _logger.LogInformation("Added claim to new user __tenant__: {@Claim}", tenantClaim);

    if (!claimResult.Succeeded)
    {
      var errorDetails = claimResult.Errors.Select(e => new ErrorDetail("tenantId", e.Description)).ToArray();
      _logger.LogError("Failed to add tenant claim for user {Email}, user and tenant registration rolled back", request.Email);
      return Result<Unit>.Invalid(errorDetails);
    }

    _logger.LogInformation("User {Email} registered successfully with tenant {TenantId}", request.Email, tenantId);

    return Result<Unit>.NoContent();
  }
}
