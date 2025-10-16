using FluentValidation.Results;

namespace Server.Web.Infrastructure;

public static class ResultExtensions
{
  /// <summary>
  /// Sends a standardized response based on Ardalis Result pattern.
  /// Handles success, error, and validation scenarios consistently.
  /// Automatically uses the correct status code based on Result.Status.
  /// </summary>
  public static async Task ResultAsync<TResult>(
    this IResponseSender ep,
    Result<TResult> result,
    CancellationToken cancellationToken = default)
  {
    await ResultAsync<TResult, TResult>(ep, result, null, cancellationToken);
  }

  /// <summary>
  /// Sends a standardized response based on Ardalis Result pattern with mapping support.
  /// Handles success, error, and validation scenarios consistently.
  /// Maps the result value using the provided mapper function.
  /// Automatically uses the correct status code based on Result.Status.
  /// </summary>
  public static async Task ResultAsync<TResult, TResponse>(
    this IResponseSender ep,
    Result<TResult> result,
    Func<TResult, TResponse>? mapper = null,
    CancellationToken cancellationToken = default)
  {
    switch (result.Status)
    {
      case ResultStatus.Ok:
        if (mapper != null)
        {
          await ep.HttpContext.Response.SendOkAsync(mapper(result), cancellation: cancellationToken);
        }
        else if (result.Value != null)
        {
          await ep.HttpContext.Response.SendOkAsync(result.Value, cancellation: cancellationToken);
        }
        else
        {
          await ep.HttpContext.Response.SendOkAsync(cancellation: cancellationToken);
        }
        break;
      case ResultStatus.Created:
        if (mapper != null)
        {
          await ep.HttpContext.Response.SendAsync(mapper(result), statusCode: StatusCodes.Status201Created, cancellation: cancellationToken);
        }
        else if (result.Value != null)
        {
          await ep.HttpContext.Response.SendAsync(result.Value, statusCode: StatusCodes.Status201Created, cancellation: cancellationToken);
        }
        else
        {
          await ep.HttpContext.Response.SendResultAsync(TypedResults.Created());
        }
        break;
      case ResultStatus.NoContent:
        await ep.HttpContext.Response.SendNoContentAsync(cancellation: cancellationToken);
        break;
      case ResultStatus.Invalid:
        foreach (var error in result.ValidationErrors)
        {
          ep.ValidationFailures.Add(new(error.Identifier, error.ErrorMessage));
        }
        await ep.HttpContext.Response.SendErrorsAsync(ep.ValidationFailures, cancellation: cancellationToken);
        break;
      case ResultStatus.NotFound:
        await ep.HttpContext.Response.SendNotFoundAsync(cancellation: cancellationToken);
        break;
      case ResultStatus.Unauthorized:
        await ep.HttpContext.Response.SendUnauthorizedAsync(cancellation: cancellationToken);
        break;
      case ResultStatus.Forbidden:
        await ep.HttpContext.Response.SendForbiddenAsync(cancellation: cancellationToken);
        break;
      case ResultStatus.Error:
        await ep.HttpContext.Response.SendErrorsAsync(new List<ValidationFailure> { new("body", string.Join(";", result.Errors)) }, cancellation: cancellationToken);
        break;
    }
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

}
