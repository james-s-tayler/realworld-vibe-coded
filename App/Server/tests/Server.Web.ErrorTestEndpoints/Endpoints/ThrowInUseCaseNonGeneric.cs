using Server.Infrastructure;
using Server.Web.ErrorTestEndpoints.UseCases;

namespace Server.Web.ErrorTestEndpoints.Endpoints;

/// <summary>
/// Test endpoint that throws an exception for non-generic Result
/// </summary>
public class ThrowInUseCaseNonGeneric(IMediator _mediator) : Endpoint<EmptyRequest>
{
  public override void Configure()
  {
    Get("/api/error-test/throw-in-use-case-non-generic");
    AllowAnonymous();
    Summary(s =>
    {
      s.Summary = "Test endpoint - throws exception for non-generic Result";
      s.Description = "This endpoint throws an exception to test non-generic Result handling.";
    });
  }

  public override async Task HandleAsync(EmptyRequest req, CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(new ThrowInUseCaseNonGenericQuery(), cancellationToken);
    await Send.ResultValueAsync(result, cancellationToken);
  }
}
