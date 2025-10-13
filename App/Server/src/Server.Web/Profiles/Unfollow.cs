using Ardalis.SharedKernel;
using Server.Core.Interfaces;
using Server.Core.UserAggregate;
using Server.Core.UserAggregate.Specifications;
using Server.Web.Infrastructure;

namespace Server.Web.Profiles;

/// <summary>
/// Unfollow user profile
/// </summary>
/// <remarks>
/// Unfollow a user by username. Authentication required.
/// </remarks>
public class Unfollow(IRepository<User> _userRepository, ICurrentUserService _currentUserService) : EndpointWithoutRequest<ProfileResponse>
{
  public override void Configure()
  {
    Delete("/api/profiles/{username}/follow");
    AuthSchemes("Token");
    Summary(s =>
    {
      s.Summary = "Unfollow user profile";
      s.Description = "Unfollow a user by username. Authentication required.";
    });
  }

  public override async Task HandleAsync(CancellationToken cancellationToken)
  {
    // Get username from route parameter
    var username = Route<string>("username") ?? string.Empty;

    var userId = _currentUserService.GetRequiredCurrentUserId();

    // Find the user to unfollow
    var userToUnfollow = await _userRepository.FirstOrDefaultAsync(
      new Server.Core.UserAggregate.UserByUsernameSpec(username), cancellationToken);

    if (userToUnfollow == null)
    {
      await Send.ResponseAsync<ConduitErrorResponse>(new ConduitErrorResponse
      {
        Errors = new ConduitErrorBody { Body = new[] { "User not found" } }
      }, 404);
      return;
    }

    // Get current user with following relationships
    var currentUser = await _userRepository.FirstOrDefaultAsync(
      new UserWithFollowingSpec(userId), cancellationToken);

    if (currentUser == null)
    {
      await Send.ResponseAsync<ConduitErrorResponse>(new ConduitErrorResponse
      {
        Errors = new ConduitErrorBody { Body = new[] { "Current user not found" } }
      }, 404);
      return;
    }

    // Check if the user is currently following the target user
    if (!currentUser.IsFollowing(userToUnfollow))
    {
      await Send.ResponseAsync<ConduitErrorResponse>(new ConduitErrorResponse
      {
        Errors = new ConduitErrorBody { Body = new[] { $"username is not being followed" } }
      }, 422);
      return;
    }

    // Unfollow the user
    currentUser.Unfollow(userToUnfollow);
    await _userRepository.SaveChangesAsync(cancellationToken);

    Response = new ProfileResponse
    {
      Profile = new ProfileDto
      {
        Username = userToUnfollow.Username,
        Bio = userToUnfollow.Bio,
        Image = userToUnfollow.Image,
        Following = false
      }
    };
  }
}
