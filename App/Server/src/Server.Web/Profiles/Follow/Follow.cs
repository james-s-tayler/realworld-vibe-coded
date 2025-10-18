using Server.Core.Interfaces;
using Server.UseCases.Profiles.Follow;
using Server.Web.Infrastructure;

namespace Server.Web.Profiles.Follow;

/// <summary>
/// Follow user profile
/// </summary>
/// <remarks>
/// Follow a user by username. Authentication required.
/// </remarks>
public class Follow(IMediator _mediator, ICurrentUserService _currentUserService) : Endpoint<FollowProfileRequest, ProfileResponse, ProfileMapper>
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

  public override async Task HandleAsync(FollowProfileRequest request, CancellationToken cancellationToken)
  {
    var userId = _currentUserService.GetRequiredCurrentUserId();

    var result = await _mediator.Send(new FollowUserCommand(request.Username, userId), cancellationToken);

    await Send.ResultAsync(result, user => Map.FromEntity(user), cancellationToken);
  }
}
