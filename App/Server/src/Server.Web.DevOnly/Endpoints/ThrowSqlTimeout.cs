namespace Server.Web.DevOnly.Endpoints;

/// <summary>
/// Test endpoint that throws a TimeoutException to simulate SQL timeout.
/// Used to verify transient error handling returns HTTP 503.
/// </summary>
public class ThrowSqlTimeout(IMediator mediator) : Endpoint<EmptyRequest>
{
  public override void Configure()
  {
    Get("throw-sql-timeout");
    Group<TestError>();
    Summary(s =>
    {
      s.Summary = "Test endpoint - throws SQL timeout exception";
      s.Description = "This endpoint throws a TimeoutException to test transient error handling (503 Service Unavailable).";
    });
  }

  public override async Task HandleAsync(EmptyRequest req, CancellationToken cancellationToken)
  {
    var result = await mediator.Send(new ThrowSqlTimeoutQuery(), cancellationToken);
    await Send.ResultValueAsync(result, cancellationToken);
  }
}
