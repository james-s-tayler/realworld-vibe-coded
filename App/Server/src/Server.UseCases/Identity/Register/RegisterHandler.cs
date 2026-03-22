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
    // Check for duplicate email
    var existingUser = await _userManager.FindByEmailAsync(request.Email);
    if (existingUser != null)
    {
      _logger.LogWarning("User registration failed for {Email}: Duplicate email", request.Email);
      return Result<Unit>.Invalid(new ErrorDetail("email", "A user has already been registered with that email"));
    }

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

    _logger.LogInformation("Registered new user");

    // Roles are seeded at application startup — assign Admin + User to the first registered user
    var defaultRoles = new List<string>
    {
      DefaultRoles.Admin,
      DefaultRoles.User,
    };

    _logger.LogDebug("Assigning roles to new user: {@Roles}", defaultRoles);

    var addRoleResult = await _userManager.AddToRolesAsync(user, defaultRoles);

    if (!addRoleResult.Succeeded)
    {
      var errorDetails = addRoleResult.Errors.Select(e => new ErrorDetail("role", e.Description)).ToArray();
      return Result<Unit>.Error(errorDetails);
    }

    _logger.LogDebug("Assigned roles to new user: {@Roles}", defaultRoles);

    _logger.LogInformation("User {Email} registered successfully", request.Email);

    return Result<Unit>.NoContent();
  }
}
