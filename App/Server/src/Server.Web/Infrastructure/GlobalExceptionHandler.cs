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
    logger.LogError(exception, "An unhandled exception occurred: {ExceptionType} - {Message}",
      exception.GetType().Name, exception.Message);

    // Mark the exception as handled to prevent automatic re-throwing
    ctx.MarkExceptionAsHandled();

    // Return error in same format as Result.CriticalError
    // This uses FastEndpoints' SendErrorsAsync which formats as problem details
    var validationFailures = new List<ValidationFailure>
    {
      new(exception.GetType().Name, exception.Message),
    };

    await ctx.HttpContext.Response.SendErrorsAsync(
      validationFailures,
      statusCode: StatusCodes.Status500InternalServerError,
      cancellation: ct);
  }
}
