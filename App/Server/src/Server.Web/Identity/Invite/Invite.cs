using Server.Infrastructure;
using Server.UseCases.Identity.Invite;

namespace Server.Web.Identity.Invite;

public class Invite(IMediator mediator) : Endpoint<InviteRequest>
{
  public override void Configure()
  {
    Post("/api/identity/invite");
    AuthSchemes("Token", "IdentityConstants.ApplicationScheme");
  }

  public override async Task HandleAsync(InviteRequest req, CancellationToken ct)
  {
    var command = new InviteCommand(req.Email, req.Password);
    var result = await mediator.Send(command, ct);

    await Send.ResultValueAsync(result, ct);
  }
}
