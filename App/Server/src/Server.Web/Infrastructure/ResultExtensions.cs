namespace Server.Web.Infrastructure;

public static class ResultExtensions
{
  /// <summary>
  /// Sends a standardized response based on Ardalis Result pattern.
  /// Handles success, error, and validation scenarios consistently.
  /// Automatically uses the correct status code based on Result.Status.
  /// </summary>
  public static async Task SendAsync<TResult>(
    this IEndpoint endpoint,
    Result<TResult> result,
    CancellationToken cancellationToken = default,
    bool treatNotFoundAsValidation = false)
  {
    await SendAsync<TResult, TResult>(endpoint, result, null, cancellationToken, treatNotFoundAsValidation);
  }

  /// <summary>
  /// Sends a standardized response based on Ardalis Result pattern with mapping support.
  /// Handles success, error, and validation scenarios consistently.
  /// Maps the result value using the provided mapper function.
  /// Automatically uses the correct status code based on Result.Status.
  /// </summary>
  public static async Task SendAsync<TResult, TResponse>(
    this IEndpoint endpoint,
    Result<TResult> result,
    Func<TResult, TResponse>? mapper = null,
    CancellationToken cancellationToken = default,
    bool treatNotFoundAsValidation = false)
  {
    var httpContext = endpoint.HttpContext;

    // Determine status code from Result.Status
    var (statusCode, errorMessage) = GetStatusCodeAndMessage(result.Status, treatNotFoundAsValidation);

    httpContext.Response.StatusCode = statusCode;

    // Handle success cases
    if (result.IsSuccess)
    {
      // NoContent (204) should not have a response body
      if (result.Status != ResultStatus.NoContent)
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

    // Handle error cases
    httpContext.Response.ContentType = "application/json";

    // Build error array
    // For Invalid status with ValidationErrors, format as "identifier message"
    // Otherwise use errors from result or default message
    string[] errorMessages;
    if (result.Status == ResultStatus.Invalid && result.ValidationErrors.Any())
    {
      errorMessages = result.ValidationErrors
        .Select(error => $"{error.Identifier} {error.ErrorMessage}")
        .ToArray();
    }
    else if (result.Errors.Any())
    {
      errorMessages = result.Errors.ToArray();
    }
    else
    {
      errorMessages = new[] { errorMessage };
    }

    var errorResponse = System.Text.Json.JsonSerializer.Serialize(new
    {
      errors = new { body = errorMessages }
    });

    await httpContext.Response.WriteAsync(errorResponse, cancellationToken);
  }

  /// <summary>
  /// Sends a validation error response (400) with custom error messages.
  /// </summary>
  public static async Task SendValidationErrorAsync(
    this IEndpoint endpoint,
    IEnumerable<string> errors,
    CancellationToken cancellationToken = default)
  {
    var httpContext = endpoint.HttpContext;
    httpContext.Response.StatusCode = 400;
    httpContext.Response.ContentType = "application/json";

    var errorResponse = System.Text.Json.JsonSerializer.Serialize(new
    {
      errors = new { body = errors.ToArray() }
    });

    await httpContext.Response.WriteAsync(errorResponse, cancellationToken);
  }

  /// <summary>
  /// Maps ResultStatus to HTTP status code and error message.
  /// </summary>
  private static (int statusCode, string errorMessage) GetStatusCodeAndMessage(
    ResultStatus status,
    bool treatNotFoundAsValidation = false)
  {
    return status switch
    {
      ResultStatus.Ok => (200, "OK"),
      ResultStatus.Created => (201, "Created"),
      ResultStatus.NoContent => (204, "No content"),
      ResultStatus.Unauthorized => (401, "Unauthorized"),
      ResultStatus.Forbidden => (403, "Forbidden"),
      ResultStatus.NotFound when treatNotFoundAsValidation => (400, "Bad request"),
      ResultStatus.NotFound => (404, "Not found"),
      ResultStatus.Invalid => (400, "Bad request"),
      ResultStatus.Conflict => (409, "Conflict"),
      ResultStatus.Unavailable => (503, "Service unavailable"),
      ResultStatus.CriticalError => (500, "Internal server error"),
      ResultStatus.Error => (400, "Bad request"),
      _ => (400, "Bad request")
    };
  }
}
