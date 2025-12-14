using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Server.Core.IdentityAggregate;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Users.GetCurrent;

public class GetCurrentUserHandler : IQueryHandler<GetCurrentUserQuery, ApplicationUser>
{
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly ILogger<GetCurrentUserHandler> _logger;

  public GetCurrentUserHandler(
    UserManager<ApplicationUser> userManager,
    ILogger<GetCurrentUserHandler> logger)
  {
    _userManager = userManager;
    _logger = logger;
  }

  public async Task<Result<ApplicationUser>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
  {
    _logger.LogInformation("Getting current user for ID {UserId}", request.UserId);

    // Look up ApplicationUser by ID
    var appUser = await _userManager.FindByIdAsync(request.UserId.ToString());

    if (appUser == null)
    {
      _logger.LogWarning("User with ID {UserId} not found", request.UserId);
      return Result<ApplicationUser>.NotFound();
    }

    _logger.LogInformation("Retrieved current user {Username}", appUser.UserName);

    return Result<ApplicationUser>.Success(appUser);
  }
}
