using Server.UseCases.ErrorTest;
using Server.Web.Infrastructure;

namespace Server.Web.ErrorTest;

/// <summary>
/// Test endpoint that calls a MediatR handler that returns Result.Error
/// This tests the standard Result error handling
/// </summary>
public class ReturnError : EndpointWithoutRequest
{
  private readonly IMediator _mediator;

  public ReturnError(IMediator mediator)
  {
    _mediator = mediator;
  }

  public override void Configure()
  {
    Get("/api/error-test/return-error");
    AllowAnonymous();
    Summary(s =>
    {
      s.Summary = "Test endpoint - returns Result.Error from MediatR handler";
      s.Description = "This endpoint calls a MediatR handler that returns Result.Error to test standard error handling";
    });
  }

  public override async Task HandleAsync(CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(new ReturnErrorQuery(), cancellationToken);
    await Send.ResultAsync(result, cancellationToken);
  }
}
