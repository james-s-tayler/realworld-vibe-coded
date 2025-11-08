using Server.Core.Interfaces;
using Server.Infrastructure;
using Server.UseCases.Users.GetCurrent;

namespace Server.Web.Users.GetCurrent;

/// <summary>
/// Get current user
/// </summary>
/// <remarks>
/// Get the currently authenticated user details.
/// </remarks>
public class GetCurrent(IMediator mediator, IUserContext userContext) : Endpoint<EmptyRequest, UserCurrentResponse, UserMapper>
{
  public override void Configure()
  {
    Get("/api/user");
    AuthSchemes("Token");
    Summary(s =>
    {
      s.Summary = "Get current user";
      s.Description = "Get the currently authenticated user details.";
    });
  }

  public override async Task HandleAsync(EmptyRequest req, CancellationToken cancellationToken)
  {
    var userId = userContext.GetRequiredCurrentUserId();

    var result = await mediator.Send(new GetCurrentUserQuery(userId), cancellationToken);

    await Send.ResultMapperAsync(
      result,
      userDto => new UserCurrentResponse
      {
        User = Map.FromEntity(userDto),
      },
      cancellationToken);
  }
}
