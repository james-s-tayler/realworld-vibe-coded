using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Server.Core.IdentityAggregate;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Users.GetUserRoles;

public class GetUserRolesHandler : IQueryHandler<GetUserRolesQuery, List<string>>
{
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly ILogger<GetUserRolesHandler> _logger;

  public GetUserRolesHandler(
    UserManager<ApplicationUser> userManager,
    ILogger<GetUserRolesHandler> logger)
  {
    _userManager = userManager;
    _logger = logger;
  }

  public async Task<Result<List<string>>> Handle(GetUserRolesQuery request, CancellationToken cancellationToken)
  {
    _logger.LogInformation("Getting roles for user {UserId}", request.UserId);

    var user = await _userManager.FindByIdAsync(request.UserId);

    if (user == null)
    {
      _logger.LogWarning("User with ID {UserId} not found", request.UserId);
      return Result<List<string>>.NotFound();
    }

    var roles = await _userManager.GetRolesAsync(user);

    _logger.LogInformation("Retrieved {RoleCount} roles for user {UserId}", roles.Count, request.UserId);

    return Result<List<string>>.Success(roles.ToList());
  }
}
