namespace Server.Web.ErrorTest;

/// <summary>
/// Test endpoint that triggers validation errors
/// </summary>
/// <remarks>
/// This endpoint is used to test validation error handling.
/// It has a fluent validator.
/// </remarks>
public class ValidationErrorValidator : Endpoint<TestValidationRequest>
{
  public override void Configure()
  {
    Verbs(Http.POST, Http.GET);
    Routes("/api/error-test/validation-error-validator");
    AllowAnonymous();
    Summary(s =>
    {
      s.Summary = "Test endpoint - validation error";
      s.Description = "This endpoint triggers validation errors using FluentValidation.";
    });
  }

  public override Task HandleAsync(TestValidationRequest testValidationRequest, CancellationToken cancellationToken)
  {
    return Task.CompletedTask;
  }
}
