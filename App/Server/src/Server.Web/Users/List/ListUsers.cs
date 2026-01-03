using Server.Infrastructure;
using Server.UseCases.Users.List;

namespace Server.Web.Users.List;

/// <summary>
/// List all users
/// </summary>
/// <remarks>
/// List all users in the system. Authentication required.
/// </remarks>
public class ListUsers(IMediator mediator) : Endpoint<EmptyRequest, UsersResponse, UserMapper>
{
  public override void Configure()
  {
    Get("/api/users");
    AuthSchemes(Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme, Microsoft.AspNetCore.Identity.IdentityConstants.BearerScheme);
    Summary(s =>
    {
      s.Summary = "List all users";
      s.Description = "List all users in the system. Authentication required.";
    });
  }

  public override async Task HandleAsync(EmptyRequest req, CancellationToken cancellationToken)
  {
    var result = await mediator.Send(new ListUsersQuery(), cancellationToken);

    await Send.ResultMapperAsync(result, async (users, ct) => await Map.FromEntityAsync(users, ct), cancellationToken);
  }
}
