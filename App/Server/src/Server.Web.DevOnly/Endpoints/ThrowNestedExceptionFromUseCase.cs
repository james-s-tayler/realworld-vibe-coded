namespace Server.Web.DevOnly.Endpoints;

/// <summary>
/// Test endpoint that throws nested exceptions from a use case to test MediatR pipeline exception handling.
/// </summary>
/// <remarks>
/// This endpoint is used to test that the global exception handler correctly
/// unwraps and includes all inner exception messages in the error response
/// when exceptions are thrown from within the MediatR pipeline.
/// </remarks>
public class ThrowNestedExceptionFromUseCase(IMediator mediator) : Endpoint<EmptyRequest>
{
  public override void Configure()
  {
    Get("throw-nested-exception-from-usecase");
    Group<TestError>();
    Summary(s =>
    {
      s.Summary = "Test endpoint - throws nested exceptions from use case";
      s.Description = "This endpoint throws a three-level nested exception inside the MediatR handler to test that the global exception handler correctly unwraps inner exceptions from the MediatR pipeline.";
    });
  }

  public override async Task HandleAsync(EmptyRequest req, CancellationToken cancellationToken)
  {
    var result = await mediator.Send(new ThrowNestedExceptionFromUseCaseQuery(), cancellationToken);
    await Send.ResultValueAsync(result, cancellationToken);
  }
}
