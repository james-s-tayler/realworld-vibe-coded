using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Server.Core.IdentityAggregate;
using Server.UseCases.Interfaces;

namespace Server.Web.Profiles;

/// <summary>
/// FastEndpoints mapper for ApplicationUser entity to ProfileResponse DTO
/// Maps domain entity to profile response with current user context for following status
/// </summary>
public class ProfileMapper : ResponseMapper<ProfileResponse, ApplicationUser>
{
  public override async Task<ProfileResponse> FromEntityAsync(ApplicationUser user, CancellationToken ct)
  {
    // Resolve current user service to get authentication context
    var currentUserService = Resolve<IUserContext>();
    var currentUserId = currentUserService.GetCurrentUserId();

    // Determine if the current user is following this profile
    bool isFollowing = false;
    if (currentUserId.HasValue)
    {
      // Get the current user to check if they are following
      var userManager = Resolve<UserManager<ApplicationUser>>();
      var currentUser = await userManager.Users
        .Include(u => u.Following)
        .FirstOrDefaultAsync(u => u.Id == currentUserId.Value, ct);

      if (currentUser != null)
      {
        isFollowing = currentUser.Following.Any(f => f.FollowedId == user.Id);
      }
    }

    return new ProfileResponse
    {
      Profile = new ProfileDto
      {
        Username = user.UserName!,
        Bio = user.Bio,
        Image = user.Image,
        Following = isFollowing,
      },
    };
  }
}
