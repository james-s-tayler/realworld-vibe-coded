using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Server.Core.IdentityAggregate;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Users.List;

public class ListUsersHandler : IQueryHandler<ListUsersQuery, List<ApplicationUser>>
{
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly ILogger<ListUsersHandler> _logger;

  public ListUsersHandler(
    UserManager<ApplicationUser> userManager,
    ILogger<ListUsersHandler> logger)
  {
    _userManager = userManager;
    _logger = logger;
  }

  public async Task<Result<List<ApplicationUser>>> Handle(ListUsersQuery request, CancellationToken cancellationToken)
  {
    _logger.LogInformation("Retrieving all users");

    var users = await _userManager.Users
      .OrderBy(u => u.UserName)
      .ToListAsync(cancellationToken);

    _logger.LogInformation("Retrieved {Count} users", users.Count);

    return Result<List<ApplicationUser>>.Success(users);
  }
}
