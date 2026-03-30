using Server.Infrastructure;
using Server.UseCases.Users.Reactivate;

namespace Server.Web.Users.Reactivate;

public class ReactivateUser(IMediator mediator) : Endpoint<ReactivateUserRequest>
{
  public override void Configure()
  {
    Put("/api/users/{UserId}/reactivate");
    AuthSchemes(Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme, Microsoft.AspNetCore.Identity.IdentityConstants.BearerScheme);
    Roles(Server.Core.IdentityAggregate.DefaultRoles.Admin);
  }

  public override async Task HandleAsync(ReactivateUserRequest req, CancellationToken ct)
  {
    var command = new ReactivateUserCommand(req.UserId);
    var result = await mediator.Send(command, ct);

    await Send.ResultValueAsync(result, ct);
  }
}
