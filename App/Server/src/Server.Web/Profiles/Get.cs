using Server.Core.Interfaces;
using Server.UseCases.Profiles.Get;
using Server.Web.Infrastructure;

namespace Server.Web.Profiles;

/// <summary>
/// Get user profile
/// </summary>
/// <remarks>
/// Get a user profile by username. Authentication optional.
/// </remarks>
public class Get(IMediator _mediator, ICurrentUserService _currentUserService) : EndpointWithoutRequest<ProfileResponse, ProfileMapper>
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

  public override async Task HandleAsync(CancellationToken cancellationToken)
  {
    // Get username from route parameter
    var username = Route<string>("username") ?? string.Empty;

    // Get current user ID if authenticated
    var currentUserId = _currentUserService.GetCurrentUserId();

    var result = await _mediator.Send(new GetProfileQuery(username, currentUserId), cancellationToken);

    await this.SendAsync(result, user => Map.FromEntity(user), cancellationToken);
  }
}
