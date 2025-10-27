namespace Server.Web.DevOnly.Endpoints;

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
    Get("throw-in-use-case");
    Group<TestError>();
    Summary(s =>
    {
      s.Summary = "Test endpoint - throws exception in use case";
      s.Description = "This endpoint throws an exception inside the MediatR handler to test exception handling pipeline behavior.";
    });
  }

  public override async Task HandleAsync(EmptyRequest req, CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(new ThrowInUseCaseQuery(), cancellationToken);
    await Send.ResultValueAsync(result, cancellationToken);
  }
}
