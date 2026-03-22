using Microsoft.AspNetCore.Identity;
using Server.Core.IdentityAggregate;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Profiles.Get;

public class GetProfileHandler(UserManager<ApplicationUser> userManager)
  : IQueryHandler<GetProfileQuery, ApplicationUser>
{
  public async Task<Result<ApplicationUser>> Handle(GetProfileQuery request, CancellationToken cancellationToken)
  {
    var user = await userManager.FindByNameAsync(request.Username);

    if (user == null)
    {
      return Result<ApplicationUser>.NotFound(request.Username);
    }

    return Result<ApplicationUser>.Success(user);
  }
}
