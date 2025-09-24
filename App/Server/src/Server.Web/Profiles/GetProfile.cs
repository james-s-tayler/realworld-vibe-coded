using System.Security.Claims;
using Ardalis.SharedKernel;
using Server.Core.UserAggregate;
using Server.Core.UserAggregate.Specifications;

namespace Server.Web.Profiles;

/// <summary>
/// Get user profile
/// </summary>
/// <remarks>
/// Get a user's profile by username. Authentication optional.
/// </remarks>
public class GetProfile(IRepository<User> _userRepository) : EndpointWithoutRequest<ProfileResponse>
{
  public override void Configure()
  {
    Get("/api/profiles/{username}");
    AllowAnonymous();
    Summary(s =>
    {
      s.Summary = "Get user profile";
      s.Description = "Get a user's profile by username. Authentication optional.";
    });
  }

  public override async Task HandleAsync(CancellationToken cancellationToken)
  {
    // Get username from route parameter
    var username = Route<string>("username") ?? string.Empty;

    if (string.IsNullOrEmpty(username))
    {
      HttpContext.Response.StatusCode = 422;
      HttpContext.Response.ContentType = "application/json";
      var errorJson = System.Text.Json.JsonSerializer.Serialize(new
      {
        errors = new { body = new[] { "Username is required" } }
      });
      await HttpContext.Response.WriteAsync(errorJson, cancellationToken);
      return;
    }

    // Find the user by username
    var userToGet = await _userRepository.FirstOrDefaultAsync(
      new Server.Core.UserAggregate.UserByUsernameSpec(username), cancellationToken);

    if (userToGet == null)
    {
      HttpContext.Response.StatusCode = 404;
      HttpContext.Response.ContentType = "application/json";
      var errorJson = System.Text.Json.JsonSerializer.Serialize(new
      {
        errors = new { body = new[] { "User not found" } }
      });
      await HttpContext.Response.WriteAsync(errorJson, cancellationToken);
      return;
    }

    // Get current user ID if authenticated
    int? currentUserId = null;
    var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
    if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
    {
      currentUserId = userId;
    }

    // Determine if current user is following this profile
    bool isFollowing = false;
    if (currentUserId.HasValue && currentUserId.Value != userToGet.Id)
    {
      var currentUser = await _userRepository.FirstOrDefaultAsync(
        new UserWithFollowingSpec(currentUserId.Value), cancellationToken);

      if (currentUser != null)
      {
        isFollowing = currentUser.IsFollowing(userToGet);
      }
    }

    Response = new ProfileResponse
    {
      Profile = new ProfileDto
      {
        Username = userToGet.Username,
        Bio = userToGet.Bio,
        Image = userToGet.Image,
        Following = isFollowing
      }
    };
  }
}
