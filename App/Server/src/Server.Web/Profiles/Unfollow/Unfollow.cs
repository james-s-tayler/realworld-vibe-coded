using Server.Core.Interfaces;
using Server.Infrastructure;
using Server.UseCases.Profiles.Unfollow;

namespace Server.Web.Profiles.Unfollow;

/// <summary>
/// Unfollow user profile
/// </summary>
/// <remarks>
/// Unfollow a user by username. Authentication required.
/// </remarks>
public class Unfollow(IMediator _mediator, IUserContext userContext) : Endpoint<UnfollowProfileRequest, ProfileResponse, ProfileMapper>
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
    var userId = userContext.GetRequiredCurrentUserId();

    var result = await _mediator.Send(new UnfollowUserCommand(request.Username, userId), cancellationToken);

    await Send.ResultMapperAsync(result, user => Map.FromEntity(user), cancellationToken);
  }
}
