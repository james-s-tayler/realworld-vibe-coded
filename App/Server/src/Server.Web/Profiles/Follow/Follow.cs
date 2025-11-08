using Server.Core.Interfaces;
using Server.Infrastructure;
using Server.UseCases.Profiles.Follow;

namespace Server.Web.Profiles.Follow;

/// <summary>
/// Follow user profile
/// </summary>
/// <remarks>
/// Follow a user by username. Authentication required.
/// </remarks>
public class Follow(IMediator mediator, IUserContext userContext) : Endpoint<FollowProfileRequest, ProfileResponse, ProfileMapper>
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
    var userId = userContext.GetRequiredCurrentUserId();

    var result = await mediator.Send(new FollowUserCommand(request.Username, userId), cancellationToken);

    await Send.ResultMapperAsync(result, async (user, ct) => await Map.FromEntityAsync(user, ct), cancellationToken);
  }
}
