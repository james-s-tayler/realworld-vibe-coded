using Microsoft.Extensions.Logging;

namespace Server.UseCases.ErrorTest;

/// <summary>
/// Test handler that returns Result.Error
/// This tests the standard Result error handling
/// </summary>
public class ReturnErrorQueryHandler : IQueryHandler<ReturnErrorQuery, Result<string>>
{
  private readonly ILogger<ReturnErrorQueryHandler> _logger;

  public ReturnErrorQueryHandler(ILogger<ReturnErrorQueryHandler> logger)
  {
    _logger = logger;
  }

  public async Task<Result<string>> Handle(ReturnErrorQuery request, CancellationToken cancellationToken)
  {
    _logger.LogInformation("Returning Result.Error from handler");
    await Task.CompletedTask;
    return Result.Error("Test error returned from MediatR handler");
  }
}
