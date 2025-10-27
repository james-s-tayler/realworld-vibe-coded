using Server.Infrastructure;
using Server.Web.ErrorTestEndpoints.UseCases;

namespace Server.Web.ErrorTestEndpoints.Endpoints;

/// <summary>
/// Test endpoint that throws a DbUpdateConcurrencyException
/// </summary>
public class ThrowConcurrency(IMediator _mediator) : Endpoint<EmptyRequest>
{
  public override void Configure()
  {
    Get("/api/error-test/throw-concurrency");
    AllowAnonymous();
    Summary(s =>
    {
      s.Summary = "Test endpoint - throws DbUpdateConcurrencyException";
      s.Description = "This endpoint throws a DbUpdateConcurrencyException to test conflict handling.";
    });
  }

  public override async Task HandleAsync(EmptyRequest req, CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(new ThrowConcurrencyQuery(), cancellationToken);
    await Send.ResultValueAsync(result, cancellationToken);
  }
}
