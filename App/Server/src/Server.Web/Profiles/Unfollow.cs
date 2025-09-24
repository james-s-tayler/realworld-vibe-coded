using System.Security.Claims;
using Ardalis.SharedKernel;
using Server.Core.UserAggregate;
using Server.Core.UserAggregate.Specifications;

namespace Server.Web.Profiles;

/// <summary>
/// Unfollow user profile
/// </summary>
/// <remarks>
/// Unfollow a user by username. Authentication required.
/// </remarks>
public class Unfollow(IRepository<User> _userRepository) : EndpointWithoutRequest<ProfileResponse>
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

    // Find the user to unfollow
    var userToUnfollow = await _userRepository.FirstOrDefaultAsync(
      new Server.Core.UserAggregate.UserByUsernameSpec(username), cancellationToken);

    if (userToUnfollow == null)
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