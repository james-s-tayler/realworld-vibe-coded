using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Core.IdentityAggregate;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Users.Deactivate;

public class DeactivateUserHandler : ICommandHandler<DeactivateUserCommand, ApplicationUser>
{
  private readonly IHttpContextAccessor _httpContextAccessor;
  private readonly ILogger<DeactivateUserHandler> _logger;

  public DeactivateUserHandler(
    IHttpContextAccessor httpContextAccessor,
    ILogger<DeactivateUserHandler> logger)
  {
    _httpContextAccessor = httpContextAccessor;
    _logger = logger;
  }

  // PV014: UserManager.SetLockoutEndDateAsync is a mutation operation, but the analyzer doesn't recognize it
  // as a repository method. Suppressing this false positive.
#pragma warning disable PV014

  public async Task<Result<ApplicationUser>> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
#pragma warning restore PV014
  {
    if (request.UserId == request.CurrentUserId)
    {
      _logger.LogWarning("User {UserId} attempted to deactivate themselves", request.CurrentUserId);
      return Result<ApplicationUser>.Forbidden(new ErrorDetail("userId", "Cannot deactivate your own account."));
    }

    var userManager = _httpContextAccessor.HttpContext!.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();

    var user = await userManager.FindByIdAsync(request.UserId.ToString());
    if (user == null)
    {
      return Result<ApplicationUser>.NotFound();
    }

    var roles = await userManager.GetRolesAsync(user);
    if (roles.Contains(DefaultRoles.Owner))
    {
      _logger.LogWarning("User {CurrentUserId} attempted to deactivate OWNER user {UserId}", request.CurrentUserId, request.UserId);
      return Result<ApplicationUser>.Forbidden(new ErrorDetail("userId", "Cannot deactivate the account owner."));
    }

    await userManager.SetLockoutEnabledAsync(user, true);
    await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);

    _logger.LogInformation("User {UserId} deactivated by {CurrentUserId}", request.UserId, request.CurrentUserId);

    return Result<ApplicationUser>.NoContent();
  }
}
