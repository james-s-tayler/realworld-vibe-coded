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
public class Unfollow(IMediator _mediator, ICurrentUserService _currentUserService) : Endpoint<UnfollowProfileRequest, ProfileResponse, ProfileMapper>
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

  public override async Task HandleAsync(UnfollowProfileRequest request, CancellationToken cancellationToken)
  {
    var userId = _currentUserService.GetRequiredCurrentUserId();

    var result = await _mediator.Send(new UnfollowUserCommand(request.Username, userId), cancellationToken);

    await Send.ResultAsync(result, user => Map.FromEntity(user), cancellationToken);
  }
}
