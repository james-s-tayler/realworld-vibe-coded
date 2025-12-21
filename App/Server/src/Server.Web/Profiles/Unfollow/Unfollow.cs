using Server.Infrastructure;
using Server.UseCases.Interfaces;
using Server.UseCases.Profiles.Unfollow;

namespace Server.Web.Profiles.Unfollow;

/// <summary>
/// Unfollow user profile
/// </summary>
/// <remarks>
/// Unfollow a user by username. Authentication required.
/// </remarks>
public class Unfollow(IMediator mediator, IUserContext userContext) : Endpoint<UnfollowProfileRequest, ProfileResponse, ProfileMapper>
{
  public override void Configure()
  {
    Delete("/api/profiles/{username}/follow");
    AuthSchemes(Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme, Microsoft.AspNetCore.Identity.IdentityConstants.BearerScheme);
    Summary(s =>
    {
      s.Summary = "Unfollow user profile";
      s.Description = "Unfollow a user by username. Authentication required.";
    });
  }

  public override async Task HandleAsync(UnfollowProfileRequest request, CancellationToken cancellationToken)
  {
    var userId = userContext.GetRequiredCurrentUserId();

    var result = await mediator.Send(new UnfollowUserCommand(request.Username, userId), cancellationToken);

    await Send.ResultMapperAsync(result, async (user, ct) => await Map.FromEntityAsync(user, ct), cancellationToken);
  }
}
