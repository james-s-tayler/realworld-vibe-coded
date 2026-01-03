using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Server.Core.IdentityAggregate;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Users.List;

public class ListUsersHandler : IQueryHandler<ListUsersQuery, List<ApplicationUser>>
{
  private readonly UserManager<ApplicationUser> _userManager;

  public ListUsersHandler(UserManager<ApplicationUser> userManager)
  {
    _userManager = userManager;
  }

  public async Task<Result<List<ApplicationUser>>> Handle(ListUsersQuery request, CancellationToken cancellationToken)
  {
    var users = await _userManager.Users
      .AsNoTracking()
      .OrderBy(u => u.UserName)
      .ToListAsync(cancellationToken);

    return Result<List<ApplicationUser>>.Success(users);
  }
}
