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
public class GetCurrent(IMediator _mediator, ICurrentUserService _currentUserService) : EndpointWithoutRequest<UserCurrentResponse, UserMapper>
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

    await this.SendAsync(result, userDto => new UserCurrentResponse
    {
      User = Map.FromEntity(userDto)
    }, cancellationToken);
  }
}

public class UserCurrentResponse
{
  public UserResponse User { get; set; } = default!;
}
