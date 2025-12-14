using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Server.Core.IdentityAggregate;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Profiles.Unfollow;

// PV014: This handler uses ASP.NET Identity's UserManager instead of the repository pattern.
// UserManager.UpdateAsync performs the database mutation internally.
#pragma warning disable PV014
public class UnfollowUserHandler(UserManager<ApplicationUser> userManager)
  : ICommandHandler<UnfollowUserCommand, ApplicationUser>
{
  public async Task<Result<ApplicationUser>> Handle(UnfollowUserCommand request, CancellationToken cancellationToken)
  {
    // Find the user to unfollow
    var userToUnfollow = await userManager.FindByNameAsync(request.Username);

    if (userToUnfollow == null)
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

    // Check if the user is currently following the target user
    if (!currentUser.IsFollowing(userToUnfollow))
    {
      return Result<ApplicationUser>.Invalid(new ErrorDetail("username", "is not being followed"));
    }

    // Unfollow the user
    currentUser.Unfollow(userToUnfollow);
    await userManager.UpdateAsync(currentUser);

    return Result<ApplicationUser>.Success(userToUnfollow);
  }
}
#pragma warning restore PV014
