using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Server.Core.IdentityAggregate;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Profiles.Follow;

// PV014: This handler uses ASP.NET Identity's UserManager instead of the repository pattern.
// UserManager.UpdateAsync performs the database mutation internally.
#pragma warning disable PV014
public class FollowUserHandler(UserManager<ApplicationUser> userManager)
  : ICommandHandler<FollowUserCommand, ApplicationUser>
{
  public async Task<Result<ApplicationUser>> Handle(FollowUserCommand request, CancellationToken cancellationToken)
  {
    // Find the user to follow
    var userToFollow = await userManager.FindByNameAsync(request.Username);

    if (userToFollow == null)
    {
      return Result<ApplicationUser>.NotFound(request.Username);
    }

    // Get current user with following relationships
    var currentUser = await userManager.Users
      .Include(u => u.Following)
      .FirstOrDefaultAsync(u => u.Id == request.CurrentUserId, cancellationToken);

    if (currentUser == null)
    {
      return Result<ApplicationUser>.NotFound(request.CurrentUserId);
    }

    // Follow the user
    currentUser.Follow(userToFollow);
    await userManager.UpdateAsync(currentUser);

    return Result<ApplicationUser>.Success(userToFollow);
  }
}
#pragma warning restore PV014
