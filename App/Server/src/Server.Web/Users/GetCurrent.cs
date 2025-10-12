using Server.Core.Interfaces;
using Server.UseCases.Users.GetCurrent;
using Server.Web.Infrastructure;

namespace Server.Web.Users;

/// <summary>
/// Get current user
/// </summary>
/// <remarks>
/// Get the currently authenticated user details.
/// </remarks>
public class GetCurrent(IMediator _mediator, ICurrentUserService _currentUserService) : EndpointWithoutRequest<UserCurrentResponse>
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
    var userId = _currentUserService.GetRequiredCurrentUserId();

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
      await HttpContext.Response.HttpContext.Response.SendAsync(new ConduitErrorResponse
      {
        Errors = new ConduitErrorBody { Body = new[] { "Unauthorized" } }
      }, 401);
      return;
    }

    await HttpContext.Response.HttpContext.Response.SendAsync(new ConduitErrorResponse
    {
      Errors = new ConduitErrorBody { Body = new[] { "Internal server error" } }
    }, 500);
  }
}

public class UserCurrentResponse
{
  public UserResponse User { get; set; } = default!;
}
