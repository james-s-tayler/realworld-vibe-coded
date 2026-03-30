using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Core.IdentityAggregate;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Users.UpdateRoles;

public class UpdateRolesHandler : ICommandHandler<UpdateRolesCommand, ApplicationUser>
{
  private readonly IHttpContextAccessor _httpContextAccessor;
  private readonly ILogger<UpdateRolesHandler> _logger;

  public UpdateRolesHandler(
    IHttpContextAccessor httpContextAccessor,
    ILogger<UpdateRolesHandler> logger)
  {
    _httpContextAccessor = httpContextAccessor;
    _logger = logger;
  }

  // PV014: UserManager role methods are mutation operations, but the analyzer doesn't recognize them
  // as repository methods. Suppressing this false positive.
#pragma warning disable PV014

  public async Task<Result<ApplicationUser>> Handle(UpdateRolesCommand request, CancellationToken cancellationToken)
#pragma warning restore PV014
  {
    var userManager = _httpContextAccessor.HttpContext!.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();

    var user = await userManager.FindByIdAsync(request.UserId.ToString());
    if (user == null)
    {
      return Result<ApplicationUser>.NotFound();
    }

    var currentRoles = await userManager.GetRolesAsync(user);

    // Cannot remove own ADMIN role
    if (request.UserId == request.CurrentUserId && currentRoles.Contains(DefaultRoles.Admin) && !request.Roles.Contains(DefaultRoles.Admin))
    {
      _logger.LogWarning("User {UserId} attempted to remove their own ADMIN role", request.CurrentUserId);
      return Result<ApplicationUser>.Forbidden(new ErrorDetail("roles", "Cannot remove your own ADMIN role."));
    }

    // Compute roles to remove (exclude USER from removal — it's always preserved)
    var rolesToRemove = currentRoles
      .Where(r => r != DefaultRoles.User && !request.Roles.Contains(r))
      .ToList();

    // Compute roles to add
    var rolesToAdd = request.Roles
      .Where(r => !currentRoles.Contains(r))
      .ToList();

    if (rolesToRemove.Count > 0)
    {
      var removeResult = await userManager.RemoveFromRolesAsync(user, rolesToRemove);
      if (!removeResult.Succeeded)
      {
        var errors = removeResult.Errors.Select(e => new ErrorDetail("roles", e.Description)).ToArray();
        return Result<ApplicationUser>.Error(errors);
      }
    }

    if (rolesToAdd.Count > 0)
    {
      var addResult = await userManager.AddToRolesAsync(user, rolesToAdd);
      if (!addResult.Succeeded)
      {
        var errors = addResult.Errors.Select(e => new ErrorDetail("roles", e.Description)).ToArray();
        return Result<ApplicationUser>.Error(errors);
      }
    }

    _logger.LogInformation(
      "Roles updated for user {UserId} by {CurrentUserId}: removed [{Removed}], added [{Added}]",
      request.UserId,
      request.CurrentUserId,
      string.Join(", ", rolesToRemove),
      string.Join(", ", rolesToAdd));

    return Result<ApplicationUser>.NoContent();
  }
}
