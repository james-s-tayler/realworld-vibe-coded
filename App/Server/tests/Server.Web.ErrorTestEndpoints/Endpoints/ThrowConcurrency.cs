using Server.Web.ErrorTestEndpoints.UseCases;

namespace Server.Web.ErrorTestEndpoints.Endpoints;

/// <summary>
/// Test endpoint that throws a DbUpdateConcurrencyException
/// </summary>
public class ThrowConcurrency(IMediator _mediator) : Endpoint<EmptyRequest>
{
  public override void Configure()
  {
    Get("/api/error-test/throw-concurrency");
    AllowAnonymous();
    Summary(s =>
    {
      s.Summary = "Test endpoint - throws DbUpdateConcurrencyException";
      s.Description = "This endpoint throws a DbUpdateConcurrencyException to test conflict handling.";
    });
  }

  public override async Task HandleAsync(EmptyRequest req, CancellationToken cancellationToken)
  {
    // The handler throws DbUpdateConcurrencyException, which is caught by ExceptionHandlingBehavior
    // and converted to Result.Conflict. We need to send the conflict response with status code 409.
    var result = await _mediator.Send(new ThrowConcurrencyQuery(), cancellationToken);

    if (result.Status == ResultStatus.Conflict)
    {
      foreach (var error in result.ValidationErrors)
      {
        AddError(new FluentValidation.Results.ValidationFailure(error.Identifier, error.ErrorMessage));
      }
      ThrowIfAnyErrors(statusCode: 409);
    }
  }
}
