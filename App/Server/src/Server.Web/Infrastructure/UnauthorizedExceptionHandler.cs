using Microsoft.AspNetCore.Diagnostics;

namespace Server.Web.Infrastructure;

/// <summary>
/// Global exception handler for unauthorized access exceptions
/// </summary>
public class UnauthorizedExceptionHandler : IExceptionHandler
{
  private readonly ILogger<UnauthorizedExceptionHandler> _logger;

  public UnauthorizedExceptionHandler(ILogger<UnauthorizedExceptionHandler> logger)
  {
    _logger = logger;
  }

  public async ValueTask<bool> TryHandleAsync(
    HttpContext httpContext,
    Exception exception,
    CancellationToken cancellationToken)
  {
    if (exception is not UnauthorizedAccessException unauthorizedException)
    {
      return false;
    }

    _logger.LogWarning(unauthorizedException, "Unauthorized access attempt");

    var errorResponse = new ConduitErrorResponse
    {
      Errors = new ConduitErrorBody { Body = new[] { "Unauthorized" } }
    };

    httpContext.Response.StatusCode = 401;
    httpContext.Response.ContentType = "application/json";
    await httpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken);
    return true;
  }
}
