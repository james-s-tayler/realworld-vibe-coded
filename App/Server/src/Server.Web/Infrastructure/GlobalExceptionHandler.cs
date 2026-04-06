using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;

namespace Server.Web.Infrastructure;

/// <summary>
/// Global post processor that catches all unhandled exceptions from endpoints
/// and returns them in the same problem details format as Result.CriticalError.
/// Classifies transient exceptions (timeouts) as 503 and concurrency conflicts as 409.
/// </summary>
public class GlobalExceptionHandler : IGlobalPostProcessor
{
  public async Task PostProcessAsync(IPostProcessorContext ctx, CancellationToken ct)
  {
    if (!ctx.HasExceptionOccurred)
    {
      return;
    }

    var exception = ctx.ExceptionDispatchInfo.SourceException;

    var logger = ctx.HttpContext.Resolve<ILogger<GlobalExceptionHandler>>();
    logger.LogError(
      exception,
      "An unhandled exception occurred: {ExceptionType} - {Message}",
      exception.GetType().Name,
      exception.Message);

    // Mark the exception as handled to prevent automatic re-throwing
    ctx.MarkExceptionAsHandled();

    // Return error in same format as Result.CriticalError
    // This uses FastEndpoints' SendErrorsAsync which formats as problem details
    // Unwrap all inner exceptions to provide full error chain
    var validationFailures = UnwrapExceptions(exception);
    var statusCode = ClassifyException(exception);

    await ctx.HttpContext.Response.SendErrorsAsync(
      validationFailures,
      statusCode: statusCode,
      cancellation: ct);
  }

  private static int ClassifyException(Exception exception)
  {
    return exception switch
    {
      DbUpdateConcurrencyException => StatusCodes.Status409Conflict,
      TimeoutException => StatusCodes.Status503ServiceUnavailable,
      _ when ContainsTransientException(exception) => StatusCodes.Status503ServiceUnavailable,
      _ => StatusCodes.Status500InternalServerError,
    };
  }

  /// <summary>
  /// Walks the exception chain looking for transient exceptions that indicate
  /// the request can be retried (e.g., SqlException with timeout error number -2).
  /// </summary>
  private static bool ContainsTransientException(Exception exception)
  {
    var current = exception;
    while (current != null)
    {
      if (current is TimeoutException)
      {
        return true;
      }

      current = current.InnerException;
    }

    return false;
  }

  /// <summary>
  /// Recursively unwraps exceptions and inner exceptions to create a list of validation failures
  /// containing the full exception chain.
  /// </summary>
  /// <param name="exception">The root exception to unwrap.</param>
  /// <returns>A list of validation failures representing the entire exception chain.</returns>
  private static List<ValidationFailure> UnwrapExceptions(Exception exception)
  {
    var failures = new List<ValidationFailure>();
    var currentException = exception;

    while (currentException != null)
    {
      failures.Add(new ValidationFailure(currentException.GetType().Name, currentException.Message));
      currentException = currentException.InnerException;
    }

    return failures;
  }
}
