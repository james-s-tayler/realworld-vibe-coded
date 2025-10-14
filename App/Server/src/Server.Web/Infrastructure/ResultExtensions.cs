namespace Server.Web.Infrastructure;

public static class ResultExtensions
{
  /// <summary>
  /// Sends a standardized response based on Ardalis Result pattern.
  /// Handles success, error, and validation scenarios consistently.
  /// </summary>
  public static async Task SendAsync<TResult>(
    this IEndpoint endpoint,
    Result<TResult> result,
    CancellationToken cancellationToken = default,
    int successStatusCode = 200,
    bool treatNotFoundAsValidation = false)
  {
    await SendAsync<TResult, TResult>(endpoint, result, null, cancellationToken, successStatusCode, treatNotFoundAsValidation);
  }

  /// <summary>
  /// Sends a standardized response based on Ardalis Result pattern with mapping support.
  /// Handles success, error, and validation scenarios consistently.
  /// Maps the result value using the provided mapper function.
  /// </summary>
  public static async Task SendAsync<TResult, TResponse>(
    this IEndpoint endpoint,
    Result<TResult> result,
    Func<TResult, TResponse>? mapper = null,
    CancellationToken cancellationToken = default,
    int successStatusCode = 200,
    bool treatNotFoundAsValidation = false)
  {
    var httpContext = endpoint.HttpContext;

    if (result.IsSuccess)
    {
      httpContext.Response.StatusCode = successStatusCode;

      if (successStatusCode != 204) // No content for 204
      {
        if (mapper != null)
        {
          var response = mapper(result.Value);
          await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
        }
        else if (result.Value != null)
        {
          await httpContext.Response.WriteAsJsonAsync(result.Value, cancellationToken);
        }
      }
      return;
    }

    // Handle different error statuses
    // Some endpoints treat NotFound as a validation error (422) for API consistency
    var (statusCode, errorMessage) = result.Status switch
    {
      ResultStatus.Unauthorized => (401, "Unauthorized"),
      ResultStatus.Forbidden => (403, "Forbidden"),
      ResultStatus.NotFound when treatNotFoundAsValidation => (422, "Validation failed"),
      ResultStatus.NotFound => (404, "Not found"),
      ResultStatus.Invalid => (422, "Validation failed"),
      ResultStatus.Conflict => (409, "Conflict"),
      ResultStatus.Unavailable => (503, "Service unavailable"),
      ResultStatus.CriticalError => (500, "Internal server error"),
      ResultStatus.Error => (422, "Validation failed"), // Map generic errors to 422 for RealWorld API convention
      _ => (400, "Bad request")
    };

    httpContext.Response.StatusCode = statusCode;
    httpContext.Response.ContentType = "application/json";

    // Build error array - use errors from result if available, otherwise use default message
    var errorMessages = result.Errors.Any()
      ? result.Errors
      : new[] { errorMessage };

    var errorResponse = System.Text.Json.JsonSerializer.Serialize(new
    {
      errors = new { body = errorMessages }
    });

    await httpContext.Response.WriteAsync(errorResponse, cancellationToken);
  }

  /// <summary>
  /// Sends a validation error response (422) with custom error messages.
  /// </summary>
  public static async Task SendValidationErrorAsync(
    this IEndpoint endpoint,
    IEnumerable<string> errors,
    CancellationToken cancellationToken = default)
  {
    var httpContext = endpoint.HttpContext;
    httpContext.Response.StatusCode = 422;
    httpContext.Response.ContentType = "application/json";

    var errorResponse = System.Text.Json.JsonSerializer.Serialize(new
    {
      errors = new { body = errors.ToArray() }
    });

    await httpContext.Response.WriteAsync(errorResponse, cancellationToken);
  }
}
