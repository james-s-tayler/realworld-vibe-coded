using Server.Web.ErrorTestEndpoints.UseCases;

namespace Server.Web.ErrorTestEndpoints.Endpoints;

/// <summary>
/// Test endpoint that throws an exception in the use case
/// </summary>
/// <remarks>
/// This endpoint is used to test the exception handling pipeline behavior.
/// It throws an InvalidOperationException inside the MediatR handler.
/// </remarks>
public class ThrowInUseCase(IMediator _mediator) : Endpoint<EmptyRequest>
{
  public override void Configure()
  {
    Get("/api/error-test/throw-in-use-case");
    AllowAnonymous();
    Summary(s =>
    {
      s.Summary = "Test endpoint - throws exception in use case";
      s.Description = "This endpoint throws an exception inside the MediatR handler to test exception handling pipeline behavior.";
    });
  }

  public override async Task HandleAsync(EmptyRequest req, CancellationToken cancellationToken)
  {
    // The handler throws InvalidOperationException, which is caught by ExceptionHandlingBehavior
    // and converted to Result.CriticalError. We need to send the error response.
    var result = await _mediator.Send(new ThrowInUseCaseQuery(), cancellationToken);

    if (result.Status == ResultStatus.CriticalError)
    {
      foreach (var error in result.ValidationErrors)
      {
        AddError(new FluentValidation.Results.ValidationFailure(error.Identifier, error.ErrorMessage));
      }
      ThrowIfAnyErrors(statusCode: 500);
    }
  }
}
