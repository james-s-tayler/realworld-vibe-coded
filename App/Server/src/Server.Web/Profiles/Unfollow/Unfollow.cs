using Server.Core.Interfaces;
using Server.UseCases.Profiles.Unfollow;
using Server.Web.Infrastructure;

namespace Server.Web.Profiles.Unfollow;

/// <summary>
/// Unfollow user profile
/// </summary>
/// <remarks>
/// Unfollow a user by username. Authentication required.
/// </remarks>
public class Unfollow(IMediator _mediator, ICurrentUserService _currentUserService) : EndpointWithoutRequest<ProfileResponse, ProfileMapper>
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

    var result = await _mediator.Send(new UnfollowUserCommand(username, userId), cancellationToken);

    await Send.ResultAsync(result, user => Map.FromEntity(user), cancellationToken);
  }
}
