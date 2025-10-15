namespace Server.Web.Infrastructure;

public static class ResultExtensions
{
  /// <summary>
  /// Sends a standardized response based on Ardalis Result pattern.
  /// Handles success, error, and validation scenarios consistently.
  /// Automatically uses the correct status code based on Result.Status.
  /// </summary>
  public static async Task ResultAsync<TResult>(
    this IResponseSender sender,
    Result<TResult> result,
    CancellationToken cancellationToken = default,
    bool treatNotFoundAsValidation = false)
  {
    await ResultAsync<TResult, TResult>(sender, result, null, cancellationToken, treatNotFoundAsValidation);
  }

  /// <summary>
  /// Sends a standardized response based on Ardalis Result pattern with mapping support.
  /// Handles success, error, and validation scenarios consistently.
  /// Maps the result value using the provided mapper function.
  /// Automatically uses the correct status code based on Result.Status.
  /// </summary>
  public static async Task ResultAsync<TResult, TResponse>(
    this IResponseSender sender,
    Result<TResult> result,
    Func<TResult, TResponse>? mapper = null,
    CancellationToken cancellationToken = default,
    bool treatNotFoundAsValidation = false)
  {
    var httpContext = sender.HttpContext;

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

    // Handle error cases using ProblemDetails
    httpContext.Response.ContentType = "application/problem+json";

    // Build ProblemDetails response
    var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
    {
      Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
      Title = GetTitleForStatus(statusCode),
      Status = statusCode,
      Instance = httpContext.Request.Path
    };

    // Add errors to extensions
    var errors = new List<object>();
    if (result.Status == ResultStatus.Invalid && result.ValidationErrors.Any())
    {
      errors.AddRange(result.ValidationErrors.Select(error => new
      {
        name = error.Identifier.ToLower(),
        reason = error.ErrorMessage
      }));
    }
    else if (result.Errors.Any())
    {
      errors.AddRange(result.Errors.Select(error => new
      {
        name = "body",
        reason = error
      }));
    }
    else
    {
      errors.Add(new
      {
        name = "body",
        reason = errorMessage
      });
    }

    problemDetails.Extensions["errors"] = errors;
    problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

    await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
  }

  /// <summary>
  /// Sends a validation error response (400) with custom error messages using ProblemDetails format.
  /// </summary>
  public static async Task ValidationErrorAsync(
    this IResponseSender sender,
    IEnumerable<string> errors,
    CancellationToken cancellationToken = default)
  {
    var httpContext = sender.HttpContext;
    httpContext.Response.StatusCode = 400;
    httpContext.Response.ContentType = "application/problem+json";

    var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
    {
      Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
      Title = "One or more validation errors occurred.",
      Status = 400,
      Instance = httpContext.Request.Path,
      Extensions =
      {
        ["errors"] = errors.Select(e => new { name = "body", reason = e }).ToList(),
        ["traceId"] = httpContext.TraceIdentifier
      }
    };

    await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
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

  /// <summary>
  /// Maps HTTP status code to a human-readable title for ProblemDetails.
  /// </summary>
  private static string GetTitleForStatus(int statusCode)
  {
    return statusCode switch
    {
      400 => "One or more validation errors occurred.",
      401 => "Unauthorized",
      403 => "Forbidden",
      404 => "Not found",
      409 => "Conflict",
      500 => "Internal server error",
      503 => "Service unavailable",
      _ => "An error occurred"
    };
  }
}
