using FluentValidation.Results;

namespace Server.Web.Infrastructure;

/// <summary>
/// Global post processor that catches all unhandled exceptions from endpoints
/// and returns them in the same problem details format as Result.CriticalError
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

    await ctx.HttpContext.Response.SendErrorsAsync(
      validationFailures,
      statusCode: StatusCodes.Status500InternalServerError,
      cancellation: ct);
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
