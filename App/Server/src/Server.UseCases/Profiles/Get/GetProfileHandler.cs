using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Server.Core.IdentityAggregate;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Profiles.Get;

public class GetProfileHandler(UserManager<ApplicationUser> userManager)
  : IQueryHandler<GetProfileQuery, ApplicationUser>
{
  public async Task<Result<ApplicationUser>> Handle(GetProfileQuery request, CancellationToken cancellationToken)
  {
    var user = await userManager.Users
      .Include(u => u.Following)
      .Include(u => u.Followers)
      .FirstOrDefaultAsync(u => u.UserName == request.Username, cancellationToken);

    if (user == null)
    {
      return Result<ApplicationUser>.NotFound(request.Username);
    }

    return Result<ApplicationUser>.Success(user);
  }
}
