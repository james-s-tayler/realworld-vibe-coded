namespace Server.Web.DevOnly.Endpoints;

/// <summary>
/// Test endpoint that throws a DbUpdateConcurrencyException
/// </summary>
public class ThrowConcurrency(IMediator _mediator) : Endpoint<EmptyRequest>
{
  public override void Configure()
  {
    Get("throw-concurrency");
    Group<TestError>();
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
