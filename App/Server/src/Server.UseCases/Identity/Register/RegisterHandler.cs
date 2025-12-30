using System.Security.Claims;
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
  private readonly ILogger<RegisterHandler> _logger;

  public RegisterHandler(
    UserManager<ApplicationUser> userManager,
    ILogger<RegisterHandler> logger)
  {
    _userManager = userManager;
    _logger = logger;
  }

  // PV014: UserManager.CreateAsync is a mutation operation, but the analyzer doesn't recognize it
  // as a repository method. Suppressing this false positive.
#pragma warning disable PV014

  public async Task<Result<Unit>> Handle(RegisterCommand request, CancellationToken cancellationToken)

#pragma warning restore PV014

  {
    _logger.LogInformation("Registering new user with email {Email}", request.Email);

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

    // Generate a unique tenant ID for this user
    var tenantId = Guid.NewGuid().ToString();

    // Add the tenant claim to the user (ClaimsStrategy uses "__tenant__" by default)
    var tenantClaim = new Claim("__tenant__", tenantId);
    var claimResult = await _userManager.AddClaimAsync(user, tenantClaim);

    if (!claimResult.Succeeded)
    {
      // If adding the claim fails, we should delete the user to maintain consistency
      await _userManager.DeleteAsync(user);
      var errorDetails = claimResult.Errors.Select(e => new ErrorDetail("tenantId", e.Description)).ToArray();
      _logger.LogError("Failed to add tenant claim for user {Email}, user registration rolled back", request.Email);
      return Result<Unit>.Invalid(errorDetails);
    }

    _logger.LogInformation("User {Email} registered successfully with tenant {TenantId}", request.Email, tenantId);

    return Result<Unit>.Success(Unit.Value);
  }
}
