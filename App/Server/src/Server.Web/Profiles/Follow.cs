using System.Security.Claims;
using Ardalis.SharedKernel;
using Server.Core.UserAggregate;
using Server.Core.UserAggregate.Specifications;

namespace Server.Web.Profiles;

/// <summary>
/// Follow user profile
/// </summary>
/// <remarks>
/// Follow a user by username. Authentication required.
/// </remarks>
public class Follow(IRepository<User> _userRepository) : EndpointWithoutRequest<ProfileResponse>
{
  public override void Configure()
  {
    Post("/api/profiles/{username}/follow");
    AuthSchemes("Token");
    Summary(s =>
    {
      s.Summary = "Follow user profile";
      s.Description = "Follow a user by username. Authentication required.";
    });
  }

  public override async Task HandleAsync(CancellationToken cancellationToken)
  {
    // Get username from route parameter
    var username = Route<string>("username") ?? string.Empty;

    var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);

    if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
    {
      HttpContext.Response.StatusCode = 401;
      HttpContext.Response.ContentType = "application/json";
      var errorJson = System.Text.Json.JsonSerializer.Serialize(new
      {
        errors = new { body = new[] { "Unauthorized" } }
      });
      await HttpContext.Response.WriteAsync(errorJson, cancellationToken);
      return;
    }

    // Find the user to follow
    var userToFollow = await _userRepository.FirstOrDefaultAsync(
      new UserByUsernameSpec(username), cancellationToken);

    if (userToFollow == null)
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

    // Get current user with following relationships
    var currentUser = await _userRepository.FirstOrDefaultAsync(
      new UserWithFollowingSpec(userId), cancellationToken);

    if (currentUser == null)
    {
      HttpContext.Response.StatusCode = 404;
      HttpContext.Response.ContentType = "application/json";
      var errorJson = System.Text.Json.JsonSerializer.Serialize(new
      {
        errors = new { body = new[] { "Current user not found" } }
      });
      await HttpContext.Response.WriteAsync(errorJson, cancellationToken);
      return;
    }

    // Follow the user
    currentUser.Follow(userToFollow);
    await _userRepository.SaveChangesAsync(cancellationToken);

    Response = new ProfileResponse
    {
      Profile = new ProfileDto
      {
        Username = userToFollow.Username,
        Bio = userToFollow.Bio,
        Image = userToFollow.Image,
        Following = true
      }
    };
  }
}
