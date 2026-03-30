using Server.Infrastructure;
using Server.UseCases.Interfaces;
using Server.UseCases.Users.Deactivate;

namespace Server.Web.Users.Deactivate;

public class DeactivateUser(IMediator mediator, IUserContext userContext) : Endpoint<DeactivateUserRequest>
{
  public override void Configure()
  {
    Put("/api/users/{UserId}/deactivate");
    AuthSchemes(Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme, Microsoft.AspNetCore.Identity.IdentityConstants.BearerScheme);
    Roles(Server.Core.IdentityAggregate.DefaultRoles.Admin);
  }

  public override async Task HandleAsync(DeactivateUserRequest req, CancellationToken ct)
  {
    var currentUserId = userContext.GetRequiredCurrentUserId();
    var command = new DeactivateUserCommand(req.UserId, currentUserId);
    var result = await mediator.Send(command, ct);

    await Send.ResultValueAsync(result, ct);
  }
}
