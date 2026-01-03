using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Server.Core.IdentityAggregate;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Users.List;

public class ListUsersHandler : IQueryHandler<ListUsersQuery, List<UserWithRoles>>
{
  private readonly UserManager<ApplicationUser> _userManager;

  public ListUsersHandler(UserManager<ApplicationUser> userManager)
  {
    _userManager = userManager;
  }

  public async Task<Result<List<UserWithRoles>>> Handle(ListUsersQuery request, CancellationToken cancellationToken)
  {
    var users = await _userManager.Users
      .AsNoTracking()
      .OrderBy(u => u.UserName)
      .ToListAsync(cancellationToken);

    var usersWithRoles = new List<UserWithRoles>();

    foreach (var user in users)
    {
      var roles = await _userManager.GetRolesAsync(user);
      usersWithRoles.Add(new UserWithRoles(user, roles.ToList()));
    }

    return Result<List<UserWithRoles>>.Success(usersWithRoles);
  }
}
