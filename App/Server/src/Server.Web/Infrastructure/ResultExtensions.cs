using FluentValidation.Results;

namespace Server.Web.Infrastructure;

public static class ResultExtensions
{
  /// <summary>
  /// Sends a standardized response based on Result pattern.
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
  /// Sends a standardized response based on Result pattern with mapping support.
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
          await ep.HttpContext.Response.SendOkAsync(mapper(result.Value), cancellation: cancellationToken);
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
          await ep.HttpContext.Response.SendAsync(mapper(result.Value), statusCode: StatusCodes.Status201Created, cancellation: cancellationToken);
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
      case ResultStatus.Conflict:
        foreach (var error in result.Errors)
        {
          ep.ValidationFailures.Add(new(nameof(ResultStatus.Conflict), error));
        }
        await ep.HttpContext.Response.SendErrorsAsync(ep.ValidationFailures, statusCode: StatusCodes.Status409Conflict, cancellation: cancellationToken);
        break;
      case ResultStatus.Error:
        await ep.HttpContext.Response.SendErrorsAsync(new List<ValidationFailure> { new("error", string.Join(";", result.Errors)) }, cancellation: cancellationToken);
        break;
      case ResultStatus.CriticalError:
        if (result.ValidationErrors.Any())
        {
          foreach (var error in result.ValidationErrors)
          {
            ep.ValidationFailures.Add(new(error.Identifier, error.ErrorMessage));
          }
          await ep.HttpContext.Response.SendErrorsAsync(ep.ValidationFailures, statusCode: StatusCodes.Status500InternalServerError, cancellation: cancellationToken);
        }
        else
        {
          await ep.HttpContext.Response.SendErrorsAsync(new List<ValidationFailure> { new("error", string.Join(";", result.Errors)) }, statusCode: StatusCodes.Status500InternalServerError, cancellation: cancellationToken);
        }
        break;
    }
  }
}
