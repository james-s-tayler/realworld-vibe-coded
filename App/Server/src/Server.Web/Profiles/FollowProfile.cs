using System.Security.Claims;
using Server.UseCases.Users;
using Server.UseCases.Users.FollowProfile;

namespace Server.Web.Profiles;

/// <summary>
/// Follow a profile
/// </summary>
/// <remarks>
/// Follow a user profile. Authentication required.
/// </remarks>
public class FollowProfile(IMediator _mediator) : EndpointWithoutRequest<FollowProfileResponse>
{
  public override void Configure()
  {
    Post("/api/profiles/{username}/follow");
    AuthSchemes("Token");
    Summary(s =>
    {
      s.Summary = "Follow a profile";
      s.Description = "Follow a user profile. Authentication required.";
    });
  }

  public override async Task HandleAsync(CancellationToken cancellationToken)
  {
    var username = Route<string>("username") ?? string.Empty;
    var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);

    if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
    {
      HttpContext.Response.StatusCode = 401;
      HttpContext.Response.ContentType = "application/json";
      var unauthorizedJson = System.Text.Json.JsonSerializer.Serialize(new
      {
        errors = new { body = new[] { "Unauthorized" } }
      });
      await HttpContext.Response.WriteAsync(unauthorizedJson, cancellationToken);
      return;
    }

    var result = await _mediator.Send(new FollowProfileCommand(userId, username), cancellationToken);

    if (result.IsSuccess)
    {
      Response = new FollowProfileResponse { Profile = result.Value };
      return;
    }

    if (result.Status == Ardalis.Result.ResultStatus.NotFound)
    {
      HttpContext.Response.StatusCode = 404;
      HttpContext.Response.ContentType = "application/json";
      var notFoundJson = System.Text.Json.JsonSerializer.Serialize(new
      {
        errors = new { body = new[] { "Profile not found" } }
      });
      await HttpContext.Response.WriteAsync(notFoundJson, cancellationToken);
      return;
    }

    HttpContext.Response.StatusCode = 400;
    HttpContext.Response.ContentType = "application/json";
    var badRequestJson = System.Text.Json.JsonSerializer.Serialize(new
    {
      errors = new { body = new[] { result.Errors.FirstOrDefault() ?? "Failed to follow profile" } }
    });
    await HttpContext.Response.WriteAsync(badRequestJson, cancellationToken);
  }
}

public class FollowProfileResponse
{
  public ProfileDto Profile { get; set; } = default!;
}
