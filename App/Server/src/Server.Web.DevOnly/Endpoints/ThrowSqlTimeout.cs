namespace Server.Web.DevOnly.Endpoints;

/// <summary>
/// Test endpoint that throws a TimeoutException simulating a SQL Server timeout
/// </summary>
public class ThrowSqlTimeout(IMediator mediator) : Endpoint<EmptyRequest>
{
  public override void Configure()
  {
    Get("throw-sql-timeout");
    Group<TestError>();
    Summary(s =>
    {
      s.Summary = "Test endpoint - throws TimeoutException simulating SQL timeout";
      s.Description = "This endpoint throws a TimeoutException to test transient error handling (503).";
    });
  }

  public override async Task HandleAsync(EmptyRequest req, CancellationToken cancellationToken)
  {
    var result = await mediator.Send(new ThrowSqlTimeoutQuery(), cancellationToken);
    await Send.ResultValueAsync(result, cancellationToken);
  }
}
