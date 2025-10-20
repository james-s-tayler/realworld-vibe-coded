using Server.UseCases.ErrorTest;
using Server.Web.Infrastructure;

namespace Server.Web.ErrorTest;

/// <summary>
/// Test endpoint that calls a MediatR handler that throws an exception
/// This tests the MediatR exception handling pipeline behavior
/// </summary>
public class ThrowInHandler : EndpointWithoutRequest
{
  private readonly IMediator _mediator;

  public ThrowInHandler(IMediator mediator)
  {
    _mediator = mediator;
  }

  public override void Configure()
  {
    Get("/api/error-test/throw-in-handler");
    AllowAnonymous();
    Summary(s =>
    {
      s.Summary = "Test endpoint - throws exception in MediatR handler";
      s.Description = "This endpoint calls a MediatR handler that throws an exception to test MediatR exception handling";
    });
  }

  public override async Task HandleAsync(CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(new ThrowInHandlerQuery(), cancellationToken);
    await Send.ResultAsync(result, cancellationToken);
  }
}
