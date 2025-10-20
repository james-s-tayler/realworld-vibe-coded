using Microsoft.Extensions.Logging;

namespace Server.UseCases.ErrorTest;

/// <summary>
/// Test handler that throws an exception
/// This tests the MediatR exception handling pipeline behavior
/// </summary>
public class ThrowInHandlerQueryHandler : IQueryHandler<ThrowInHandlerQuery, Result<string>>
{
  private readonly ILogger<ThrowInHandlerQueryHandler> _logger;

  public ThrowInHandlerQueryHandler(ILogger<ThrowInHandlerQueryHandler> logger)
  {
    _logger = logger;
  }

  public async Task<Result<string>> Handle(ThrowInHandlerQuery request, CancellationToken cancellationToken)
  {
    _logger.LogInformation("About to throw exception in handler");
    await Task.CompletedTask;
    throw new InvalidOperationException("Test exception thrown in MediatR handler");
  }
}
