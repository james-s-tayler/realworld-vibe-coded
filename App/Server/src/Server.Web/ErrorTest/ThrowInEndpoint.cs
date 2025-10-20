namespace Server.Web.ErrorTest;

/// <summary>
/// Test endpoint that throws an exception directly in the endpoint handler
/// This tests the global exception handler
/// </summary>
public class ThrowInEndpoint : EndpointWithoutRequest
{
  public override void Configure()
  {
    Get("/api/error-test/throw-in-endpoint");
    AllowAnonymous();
    Summary(s =>
    {
      s.Summary = "Test endpoint - throws exception in endpoint";
      s.Description = "This endpoint throws an exception directly in the endpoint handler to test global exception handling";
    });
  }

  public override async Task HandleAsync(CancellationToken cancellationToken)
  {
    await Task.CompletedTask;
    throw new InvalidOperationException("Test exception thrown in endpoint");
  }
}
