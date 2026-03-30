using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Core.IdentityAggregate;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Users.Reactivate;

public class ReactivateUserHandler : ICommandHandler<ReactivateUserCommand, ApplicationUser>
{
  private readonly IHttpContextAccessor _httpContextAccessor;
  private readonly ILogger<ReactivateUserHandler> _logger;

  public ReactivateUserHandler(
    IHttpContextAccessor httpContextAccessor,
    ILogger<ReactivateUserHandler> logger)
  {
    _httpContextAccessor = httpContextAccessor;
    _logger = logger;
  }

  // PV014: UserManager.SetLockoutEndDateAsync is a mutation operation, but the analyzer doesn't recognize it
  // as a repository method. Suppressing this false positive.
#pragma warning disable PV014

  public async Task<Result<ApplicationUser>> Handle(ReactivateUserCommand request, CancellationToken cancellationToken)
#pragma warning restore PV014
  {
    var userManager = _httpContextAccessor.HttpContext!.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();

    var user = await userManager.FindByIdAsync(request.UserId.ToString());
    if (user == null)
    {
      return Result<ApplicationUser>.NotFound();
    }

    await userManager.SetLockoutEndDateAsync(user, null);
    await userManager.ResetAccessFailedCountAsync(user);

    _logger.LogInformation("User {UserId} reactivated", request.UserId);

    return Result<ApplicationUser>.NoContent();
  }
}
