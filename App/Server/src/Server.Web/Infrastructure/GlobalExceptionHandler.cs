using FluentValidation.Results;
using Microsoft.AspNetCore.Diagnostics;

namespace Server.Web.Infrastructure;

/// <summary>
/// Global exception handler that catches all unhandled exceptions from endpoints
/// and returns them in the same problem details format as Result.CriticalError
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
  private readonly ILogger<GlobalExceptionHandler> _logger;

  public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
  {
    _logger = logger;
  }

  public async ValueTask<bool> TryHandleAsync(
    HttpContext httpContext,
    Exception exception,
    CancellationToken cancellationToken)
  {
    // Let specific exception handlers (like UnauthorizedExceptionHandler) handle their exceptions first
    if (exception is UnauthorizedAccessException)
    {
      return false;
    }

    _logger.LogError(exception, "An unhandled exception occurred: {ExceptionType} - {Message}",
      exception.GetType().Name, exception.Message);

    // Return error in same format as Result.CriticalError
    // This uses FastEndpoints' SendErrorsAsync which formats as problem details
    httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

    var validationFailures = new List<ValidationFailure>
    {
      new(exception.GetType().Name, exception.Message)
    };

    await httpContext.Response.SendErrorsAsync(validationFailures,
      statusCode: StatusCodes.Status500InternalServerError,
      cancellation: cancellationToken);

    return true;
  }
}
