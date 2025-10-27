using Server.Core.Interfaces;
using Server.Core.UserAggregate;
using Server.Core.UserAggregate.Specifications;
using Server.SharedKernel.Persistence;

namespace Server.Web.Profiles;

/// <summary>
/// FastEndpoints mapper for User entity to ProfileResponse DTO
/// Maps domain entity to profile response with current user context for following status
/// </summary>
public class ProfileMapper : ResponseMapper<ProfileResponse, User>
{
  public override ProfileResponse FromEntity(User user)
  {
    // Resolve current user service to get authentication context
    var currentUserService = Resolve<IUserContext>();
    var currentUserId = currentUserService.GetCurrentUserId();

    // Determine if the current user is following this profile
    bool isFollowing = false;
    if (currentUserId.HasValue)
    {
      // Get the current user to check if they are following
      var userRepository = Resolve<IRepository<User>>();
      var currentUser = userRepository.FirstOrDefaultAsync(
        new UserWithFollowingSpec(currentUserId.Value)).GetAwaiter().GetResult();

      if (currentUser != null)
      {
        isFollowing = currentUser.IsFollowing(user);
      }
    }

    return new ProfileResponse
    {
      Profile = new ProfileDto
      {
        Username = user.Username,
        Bio = user.Bio,
        Image = user.Image,
        Following = isFollowing
      }
    };
  }
}
