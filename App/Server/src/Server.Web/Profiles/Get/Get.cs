using Server.Core.Interfaces;
using Server.Infrastructure;
using Server.UseCases.Profiles.Get;

namespace Server.Web.Profiles.Get;

/// <summary>
/// Get user profile
/// </summary>
/// <remarks>
/// Get a user profile by username. Authentication optional.
/// </remarks>
public class Get(IMediator _mediator, IUserContext userContext) : Endpoint<GetProfileRequest, ProfileResponse, ProfileMapper>
{
  public override void Configure()
  {
    Get("/api/profiles/{username}");
    AllowAnonymous();
    Summary(s =>
    {
      s.Summary = "Get user profile";
      s.Description = "Get a user profile by username. Authentication optional.";
    });
  }

  public override async Task HandleAsync(GetProfileRequest request, CancellationToken cancellationToken)
  {
    // Get current user ID if authenticated
    var currentUserId = userContext.GetCurrentUserId();

    var result = await _mediator.Send(new GetProfileQuery(request.Username, currentUserId), cancellationToken);

    await Send.ResultMapperAsync(result, user => Map.FromEntity(user), cancellationToken);
  }
}
