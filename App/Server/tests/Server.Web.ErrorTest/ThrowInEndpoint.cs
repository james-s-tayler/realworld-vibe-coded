namespace Server.Web.ErrorTest;

/// <summary>
/// Test endpoint that throws an exception directly in the endpoint handler
/// </summary>
/// <remarks>
/// This endpoint is used to test the global exception handler behavior.
/// It throws an InvalidOperationException directly in the endpoint, not in a use case.
/// </remarks>
public class ThrowInEndpoint : Endpoint<EmptyRequest>
{
  public override void Configure()
  {
    Get("/api/error-test/throw-in-endpoint");
    AllowAnonymous();
    Summary(s =>
    {
      s.Summary = "Test endpoint - throws exception in endpoint";
      s.Description = "This endpoint throws an exception directly in the endpoint handler to test global exception handling behavior.";
    });
  }

  public override Task HandleAsync(EmptyRequest req, CancellationToken cancellationToken)
  {
    throw new InvalidOperationException("This is a test exception thrown in the endpoint");
  }
}
