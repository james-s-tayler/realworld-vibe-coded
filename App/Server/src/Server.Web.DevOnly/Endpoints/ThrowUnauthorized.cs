namespace Server.Web.DevOnly.Endpoints;

/// <summary>
/// Test endpoint that throws an exception directly in the endpoint handler
/// </summary>
/// <remarks>
/// This endpoint is used to test problem details format for auth failures.
/// It throws an InvalidOperationException directly in the endpoint, not in a use case.
/// </remarks>
public class ThrowUnauthorized : Endpoint<EmptyRequest>
{
  public override void Configure()
  {
    Get("throw-unauthorized");
    Group<TestAuth>();
    Summary(s =>
    {
      s.Summary = "Test endpoint - throws exception in endpoint";
      s.Description = "This endpoint throws an exception due to needing authorization.";
    });
  }

  public override Task HandleAsync(EmptyRequest req, CancellationToken cancellationToken)
  {
    return Task.CompletedTask;
  }
}
