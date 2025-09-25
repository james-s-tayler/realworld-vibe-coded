using System.Security.Claims;
using Ardalis.SharedKernel;
using Server.Core.UserAggregate;
using Server.Core.UserAggregate.Specifications;

namespace Server.Web.Profiles;

/// <summary>
/// Get user profile
/// </summary>
/// <remarks>
/// Get a user profile by username. Authentication optional.
/// </remarks>
public class Get(IRepository<User> _userRepository) : EndpointWithoutRequest<ProfileResponse>
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

    // Find the user
    var userToView = await _userRepository.FirstOrDefaultAsync(
      new UserByUsernameSpec(username), cancellationToken);

    if (userToView == null)
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

    // Check if current user is authenticated and following this user
    bool isFollowing = false;
    var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
    if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var currentUserId))
    {
      var currentUser = await _userRepository.FirstOrDefaultAsync(
        new UserWithFollowingSpec(currentUserId), cancellationToken);
      
      if (currentUser != null)
      {
        isFollowing = currentUser.IsFollowing(userToView);
      }
    }

    Response = new ProfileResponse
    {
      Profile = new ProfileDto
      {
        Username = userToView.Username,
        Bio = userToView.Bio,
        Image = userToView.Image,
        Following = isFollowing
      }
    };
  }
}