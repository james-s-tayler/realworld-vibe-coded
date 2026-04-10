using Server.Infrastructure;
using Server.UseCases.Config.Get;

namespace Server.Web.Config.Get;

public class GetConfig(IMediator mediator) : Endpoint<EmptyRequest, ConfigResponse, ConfigMapper>
{
  public override void Configure()
  {
    Get("/api/config");
    AuthSchemes(Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme, Microsoft.AspNetCore.Identity.IdentityConstants.BearerScheme);
    Summary(s =>
    {
      s.Summary = "Get client configuration";
      s.Description = "Returns configuration values for the client application.";
    });
  }

  public override async Task HandleAsync(EmptyRequest req, CancellationToken cancellationToken)
  {
    var result = await mediator.Send(new GetConfigQuery(), cancellationToken);

    await Send.ResultMapperAsync(result, Map.FromEntityAsync, cancellationToken);
  }
}
