using FluentValidation.Results;

namespace Server.Web.DevOnly.Endpoints;

/// <summary>
/// Test endpoint that triggers validation errors
/// </summary>
/// <remarks>
/// This endpoint is used to test validation error handling.
/// It calls AddError() and then ThrowIfAnyErrors() to simulate validation failures.
/// </remarks>
public class ValidationErrorEndpoint : Endpoint<EmptyRequest>
{
  public override void Configure()
  {
    Get("/api/error-test/validation-error-endpoint");
    AllowAnonymous();
    Summary(s =>
    {
      s.Summary = "Test endpoint - validation error";
      s.Description = "This endpoint triggers validation errors using AddError() and ThrowIfAnyErrors().";
    });
  }

  public override Task HandleAsync(EmptyRequest request, CancellationToken cancellationToken)
  {
    AddError(new ValidationFailure("field1", "This is a test validation error for field1"));
    AddError(new ValidationFailure("field2", "This is a test validation error for field2"));
    ThrowIfAnyErrors();
    return Task.CompletedTask;
  }
}
