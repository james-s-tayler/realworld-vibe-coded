using Ardalis.SharedKernel;
using Server.Core.Interfaces;
using Server.Core.UserAggregate;
using Server.Core.UserAggregate.Specifications;
using Server.Web.Infrastructure;

namespace Server.Web.Profiles;

/// <summary>
/// Get user profile
/// </summary>
/// <remarks>
/// Get a user profile by username. Authentication optional.
/// </remarks>
public class Get(IRepository<User> _userRepository, ICurrentUserService _currentUserService) : EndpointWithoutRequest<ProfileResponse>
{
  public override void Configure()
  {
    Get("/api/profiles/{username}");
    AllowAnonymous();
    Summary(s =>
    {
      s.Summary = "Get user profile";
      s.Description = "Get a user profile by username. Authentication optional.";
    });
  }

  public override async Task HandleAsync(CancellationToken cancellationToken)
  {
    // Get username from route parameter
    var username = Route<string>("username") ?? string.Empty;

    // Get current user ID if authenticated
    var currentUserId = _currentUserService.GetCurrentUserId();

    // Find the user profile
    var user = await _userRepository.FirstOrDefaultAsync(
      new UserByUsernameWithFollowingSpec(username), cancellationToken);

    if (user == null)
    {
      await Send.ResponseAsync<ConduitErrorResponse>(new ConduitErrorResponse
      {
        Errors = new ConduitErrorBody { Body = new[] { "User not found" } }
      }, 404);
      return;
    }

    // Determine if the current user is following this profile
    bool isFollowing = false;
    if (currentUserId.HasValue)
    {
      // Get the current user to check if they are following
      var currentUser = await _userRepository.FirstOrDefaultAsync(
        new UserWithFollowingSpec(currentUserId.Value), cancellationToken);

      if (currentUser != null)
      {
        isFollowing = currentUser.IsFollowing(user);
      }
    }

    Response = new ProfileResponse
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
