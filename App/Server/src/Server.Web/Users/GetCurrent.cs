using System.Security.Claims;
using Server.UseCases.Users.GetCurrent;

namespace Server.Web.Users;

/// <summary>
/// Get current user
/// </summary>
/// <remarks>
/// Get the currently authenticated user details.
/// </remarks>
public class GetCurrent(IMediator _mediator) : EndpointWithoutRequest<UserCurrentResponse>
{
  public override void Configure()
  {
    Get("/api/user");
    AuthSchemes("Token");
    Summary(s =>
    {
      s.Summary = "Get current user";
      s.Description = "Get the currently authenticated user details.";
    });
  }

  public override async Task HandleAsync(CancellationToken cancellationToken)
  {
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

    var result = await _mediator.Send(new GetCurrentUserQuery(userId), cancellationToken);

    if (result.IsSuccess)
    {
      var userDto = result.Value;
      Response = new UserCurrentResponse
      {
        User = new UserResponse
        {
          Email = userDto.Email,
          Username = userDto.Username,
          Bio = userDto.Bio,
          Image = userDto.Image,
          Token = userDto.Token
        }
      };
      return;
    }

    if (result.Status == ResultStatus.NotFound)
    {
      HttpContext.Response.StatusCode = 401;
      HttpContext.Response.ContentType = "application/json";
      var notFoundJson = System.Text.Json.JsonSerializer.Serialize(new
      {
        errors = new { body = new[] { "Unauthorized" } }
      });
      await HttpContext.Response.WriteAsync(notFoundJson, cancellationToken);
      return;
    }

    HttpContext.Response.StatusCode = 500;
    HttpContext.Response.ContentType = "application/json";
    var serverErrorJson = System.Text.Json.JsonSerializer.Serialize(new
    {
      errors = new { body = new[] { "Internal server error" } }
    });
    await HttpContext.Response.WriteAsync(serverErrorJson, cancellationToken);
  }
}

public class UserCurrentResponse
{
  public UserResponse User { get; set; } = default!;
}