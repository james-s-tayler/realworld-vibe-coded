using Server.Infrastructure;
using Server.UseCases.Interfaces;
using Server.UseCases.Profiles.Get;

namespace Server.Web.Profiles.Get;

/// <summary>
/// Get user profile
/// </summary>
/// <remarks>
/// Get a user profile by username. Authentication required.
/// </remarks>
public class Get(IMediator mediator, IUserContext userContext) : Endpoint<GetProfileRequest, ProfileResponse, ProfileMapper>
{
  public override void Configure()
  {
    Get("/api/profiles/{username}");
    AuthSchemes(Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme, Microsoft.AspNetCore.Identity.IdentityConstants.BearerScheme);
    Summary(s =>
    {
      s.Summary = "Get user profile";
      s.Description = "Get a user profile by username. Authentication required.";
    });
  }

  public override async Task HandleAsync(GetProfileRequest request, CancellationToken cancellationToken)
  {
    // Get current user ID (authentication required)
    var currentUserId = userContext.GetRequiredCurrentUserId();

    var result = await mediator.Send(new GetProfileQuery(request.Username, currentUserId), cancellationToken);

    await Send.ResultMapperAsync(result, async (user, ct) => await Map.FromEntityAsync(user, ct), cancellationToken);
  }
}
