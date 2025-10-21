namespace Server.Web.ErrorTest;

/// <summary>
/// Test endpoint that triggers validation errors
/// </summary>
/// <remarks>
/// This endpoint is used to test validation error handling.
/// It calls AddError() and then ThrowIfAnyErrors() to simulate validation failures.
/// </remarks>
public class ValidationError : Endpoint<EmptyRequest>
{
  public override void Configure()
  {
    Get("/api/error-test/validation-error");
    AllowAnonymous();
    Summary(s =>
    {
      s.Summary = "Test endpoint - validation error";
      s.Description = "This endpoint triggers validation errors using AddError() and ThrowIfAnyErrors().";
    });
  }

  public override Task HandleAsync(EmptyRequest req, CancellationToken cancellationToken)
  {
    AddError("field1", "This is a test validation error for field1");
    AddError("field2", "This is a test validation error for field2");
    ThrowIfAnyErrors();
    return Task.CompletedTask;
  }
}
