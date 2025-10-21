using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;

namespace Server.Web.Infrastructure;

/// <summary>
/// Global exception handler for all unhandled exceptions
/// Returns consistent problem details format
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
    _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);

    httpContext.Response.StatusCode = 500;
    httpContext.Response.ContentType = "application/problem+json";

    var errorResponse = JsonSerializer.Serialize(new
    {
      type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
      title = "Internal Server Error",
      status = 500,
      detail = exception.Message,
      errors = new { error = new[] { exception.Message } }
    }, new JsonSerializerOptions
    {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    });

    await httpContext.Response.WriteAsync(errorResponse, cancellationToken);
    return true;
  }
}
