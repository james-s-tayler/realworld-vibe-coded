using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Server.SharedKernel.Result;

namespace Server.Infrastructure;

public static class ResultExtensions
{
  /// <summary>
  /// Sends a standardized response based on Result pattern with a value.
  /// Handles success, error, and validation scenarios consistently.
  /// Handles Result&lt;Unit&gt; (empty results) by not sending a value.
  /// Automatically uses the correct status code based on Result.Status.
  /// </summary>
  public static async Task ResultValueAsync<TResult>(
    this IResponseSender ep,
    Result<TResult> result,
    CancellationToken cancellationToken = default)
  {
    await ResultAsync<TResult, TResult>(ep, result, null, cancellationToken);
  }

  /// <summary>
  /// Sends a standardized response based on Result pattern with mapping support.
  /// Handles success, error, and validation scenarios consistently.
  /// Maps the result value using the provided mapper function before sending.
  /// Automatically uses the correct status code based on Result.Status.
  /// </summary>
  public static async Task ResultMapperAsync<TResult, TResponse>(
    this IResponseSender ep,
    Result<TResult> result,
    Func<TResult, TResponse> mapper,
    CancellationToken cancellationToken = default)
  {
    await ResultAsync(ep, result, mapper, cancellationToken);
  }

  private static async Task ResultAsync<TResult, TResponse>(
    this IResponseSender ep,
    Result<TResult> result,
    Func<TResult, TResponse>? mapper,
    CancellationToken cancellationToken)
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
        foreach (var error in result.ErrorDetails)
        {
          ep.ValidationFailures.Add(new(error.Identifier, error.ErrorMessage));
        }
        await ep.HttpContext.Response.SendErrorsAsync(ep.ValidationFailures, cancellation: cancellationToken);
        break;
      case ResultStatus.NotFound:
        if (result.ErrorDetails.Any())
        {
          foreach (var error in result.ErrorDetails)
          {
            ep.ValidationFailures.Add(new(error.Identifier, error.ErrorMessage));
          }
          await ep.HttpContext.Response.SendErrorsAsync(ep.ValidationFailures, statusCode: StatusCodes.Status404NotFound, cancellation: cancellationToken);
        }
        else
        {
          await ep.HttpContext.Response.SendNotFoundAsync(cancellation: cancellationToken);
        }
        break;
      case ResultStatus.Unauthorized:
        if (result.ErrorDetails.Any())
        {
          foreach (var error in result.ErrorDetails)
          {
            ep.ValidationFailures.Add(new(error.Identifier, error.ErrorMessage));
          }
          await ep.HttpContext.Response.SendErrorsAsync(ep.ValidationFailures, statusCode: StatusCodes.Status401Unauthorized, cancellation: cancellationToken);
        }
        else
        {
          await ep.HttpContext.Response.SendUnauthorizedAsync(cancellation: cancellationToken);
        }
        break;
      case ResultStatus.Forbidden:
        if (result.ErrorDetails.Any())
        {
          foreach (var error in result.ErrorDetails)
          {
            ep.ValidationFailures.Add(new(error.Identifier, error.ErrorMessage));
          }
          await ep.HttpContext.Response.SendErrorsAsync(ep.ValidationFailures, statusCode: StatusCodes.Status403Forbidden, cancellation: cancellationToken);
        }
        else
        {
          await ep.HttpContext.Response.SendForbiddenAsync(cancellation: cancellationToken);
        }
        break;
      case ResultStatus.Conflict:
        foreach (var error in result.ErrorDetails)
        {
          ep.ValidationFailures.Add(new(error.Identifier, error.ErrorMessage));
        }
        await ep.HttpContext.Response.SendErrorsAsync(ep.ValidationFailures, statusCode: StatusCodes.Status409Conflict, cancellation: cancellationToken);
        break;
      case ResultStatus.Error:
        foreach (var error in result.ErrorDetails)
        {
          ep.ValidationFailures.Add(new(error.Identifier, error.ErrorMessage));
        }
        await ep.HttpContext.Response.SendErrorsAsync(ep.ValidationFailures, cancellation: cancellationToken);
        break;
      case ResultStatus.CriticalError:
        foreach (var error in result.ErrorDetails)
        {
          ep.ValidationFailures.Add(new(error.Identifier, error.ErrorMessage));
        }
        await ep.HttpContext.Response.SendErrorsAsync(ep.ValidationFailures, statusCode: StatusCodes.Status500InternalServerError, cancellation: cancellationToken);
        break;
      case ResultStatus.Unavailable:
        foreach (var error in result.ErrorDetails)
        {
          ep.ValidationFailures.Add(new(error.Identifier, error.ErrorMessage));
        }
        await ep.HttpContext.Response.SendErrorsAsync(ep.ValidationFailures, statusCode: StatusCodes.Status503ServiceUnavailable, cancellation: cancellationToken);
        break;
    }
  }
}
