using Server.Infrastructure;
using Server.UseCases.Identity.Register;

namespace Server.Web.Identity.Register;

public class Register(IMediator mediator) : Endpoint<RegisterRequest>
{
  public override void Configure()
  {
    Post("/api/identity/register");
    AllowAnonymous();
  }

  public override async Task HandleAsync(RegisterRequest req, CancellationToken ct)
  {
    var command = new RegisterCommand(req.Email, req.Password);
    var result = await mediator.Send(command, ct);

    await Send.ResultValueAsync(result, ct);
  }
}
