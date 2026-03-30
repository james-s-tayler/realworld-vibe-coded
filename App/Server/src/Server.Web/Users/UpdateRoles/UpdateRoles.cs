using Server.Infrastructure;
using Server.UseCases.Interfaces;
using Server.UseCases.Users.UpdateRoles;

namespace Server.Web.Users.UpdateRoles;

public class UpdateRoles(IMediator mediator, IUserContext userContext) : Endpoint<UpdateRolesRequest>
{
  public override void Configure()
  {
    Put("/api/users/{UserId}/roles");
    AuthSchemes(Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme, Microsoft.AspNetCore.Identity.IdentityConstants.BearerScheme);
    Roles(Server.Core.IdentityAggregate.DefaultRoles.Admin);
  }

  public override async Task HandleAsync(UpdateRolesRequest req, CancellationToken ct)
  {
    var currentUserId = userContext.GetRequiredCurrentUserId();
    var command = new UpdateRolesCommand(req.UserId, currentUserId, req.Roles);
    var result = await mediator.Send(command, ct);

    await Send.ResultValueAsync(result, ct);
  }
}
