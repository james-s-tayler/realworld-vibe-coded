using Server.Infrastructure;
using Server.Web.ErrorTestEndpoints.UseCases;

namespace Server.Web.ErrorTestEndpoints.Endpoints;

/// <summary>
/// Test endpoint that throws a DbUpdateConcurrencyException for non-generic Result
/// </summary>
public class ThrowConcurrencyNonGeneric(IMediator _mediator) : Endpoint<EmptyRequest>
{
  public override void Configure()
  {
    Get("/api/error-test/throw-concurrency-non-generic");
    AllowAnonymous();
    Summary(s =>
    {
      s.Summary = "Test endpoint - throws DbUpdateConcurrencyException for non-generic Result";
      s.Description = "This endpoint throws a DbUpdateConcurrencyException to test non-generic Result conflict handling.";
    });
  }

  public override async Task HandleAsync(EmptyRequest req, CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(new ThrowConcurrencyNonGenericQuery(), cancellationToken);
    await Send.ResultValueAsync(result, cancellationToken);
  }
}
