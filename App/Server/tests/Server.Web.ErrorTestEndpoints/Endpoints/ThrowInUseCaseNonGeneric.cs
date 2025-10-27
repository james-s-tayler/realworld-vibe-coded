using Server.Web.ErrorTestEndpoints.UseCases;

namespace Server.Web.ErrorTestEndpoints.Endpoints;

/// <summary>
/// Test endpoint that throws an exception for non-generic Result
/// </summary>
public class ThrowInUseCaseNonGeneric(IMediator _mediator) : Endpoint<EmptyRequest>
{
  public override void Configure()
  {
    Get("/api/error-test/throw-in-use-case-non-generic");
    AllowAnonymous();
    Summary(s =>
    {
      s.Summary = "Test endpoint - throws exception for non-generic Result";
      s.Description = "This endpoint throws an exception to test non-generic Result handling.";
    });
  }

  public override async Task HandleAsync(EmptyRequest req, CancellationToken cancellationToken)
  {
    // The handler throws InvalidOperationException, which is caught by ExceptionHandlingBehavior
    // and converted to Result.CriticalError. We need to send the error response.
    var result = await _mediator.Send(new ThrowInUseCaseNonGenericQuery(), cancellationToken);

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
