using Server.Infrastructure;
using Server.UseCases.Interfaces;
using Server.UseCases.Profiles.Unfollow;

namespace Server.Web.Profiles.Unfollow;

public class Unfollow(IMediator mediator, IUserContext userContext) : Endpoint<UnfollowRequest, ProfileResponse, ProfileMapper>
{
  public override void Configure()
  {
    Delete("/api/profiles/{username}/follow");
    AuthSchemes(Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme, Microsoft.AspNetCore.Identity.IdentityConstants.BearerScheme);
  }

  public override async Task HandleAsync(UnfollowRequest request, CancellationToken cancellationToken)
  {
    var userId = userContext.GetRequiredCurrentUserId();

    var result = await mediator.Send(
      new UnfollowCommand(request.Username, userId),
      cancellationToken);

    await Send.ResultMapperAsync(
      result,
      async (profileResult, ct) => await Map.FromEntityAsync(profileResult, ct),
      cancellationToken);
  }
}
