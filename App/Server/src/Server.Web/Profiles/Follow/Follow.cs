using Server.Infrastructure;
using Server.UseCases.Interfaces;
using Server.UseCases.Profiles.Follow;

namespace Server.Web.Profiles.Follow;

public class Follow(IMediator mediator, IUserContext userContext) : Endpoint<FollowRequest, ProfileResponse, ProfileMapper>
{
  public override void Configure()
  {
    Post("/api/profiles/{username}/follow");
    AuthSchemes(Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme, Microsoft.AspNetCore.Identity.IdentityConstants.BearerScheme);
  }

  public override async Task HandleAsync(FollowRequest request, CancellationToken cancellationToken)
  {
    var userId = userContext.GetRequiredCurrentUserId();

    var result = await mediator.Send(
      new FollowCommand(request.Username, userId),
      cancellationToken);

    await Send.ResultMapperAsync(
      result,
      async (profileResult, ct) => await Map.FromEntityAsync(profileResult, ct),
      cancellationToken);
  }
}
