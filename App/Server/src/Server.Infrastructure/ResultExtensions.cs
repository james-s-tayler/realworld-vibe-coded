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
        await SendErrorResponseAsync(ep, result, StatusCodes.Status400BadRequest);
        break;
      case ResultStatus.NotFound:
        await SendErrorResponseAsync(ep, result, StatusCodes.Status404NotFound);
        break;
      case ResultStatus.Unauthorized:
        await SendErrorResponseAsync(ep, result, StatusCodes.Status401Unauthorized);
        break;
      case ResultStatus.Forbidden:
        await SendErrorResponseAsync(ep, result, StatusCodes.Status403Forbidden);
        break;
      case ResultStatus.Conflict:
        await SendErrorResponseAsync(ep, result, StatusCodes.Status409Conflict);
        break;
      case ResultStatus.Error:
        await SendErrorResponseAsync(ep, result, StatusCodes.Status400BadRequest);
        break;
      case ResultStatus.CriticalError:
        await SendErrorResponseAsync(ep, result, StatusCodes.Status500InternalServerError);
        break;
      case ResultStatus.Unavailable:
        await SendErrorResponseAsync(ep, result, StatusCodes.Status503ServiceUnavailable);
        break;
    }
  }

  /// <summary>
  /// Sends an error response with error details and the specified status code.
  /// </summary>
  private static async Task SendErrorResponseAsync<TResult>(
    IResponseSender ep,
    Result<TResult> result,
    int statusCode)
  {
    foreach (var error in result.ErrorDetails)
    {
      ep.ValidationFailures.Add(new(error.Identifier, error.ErrorMessage));
    }

    await ep.HttpContext.Response.SendErrorsAsync(ep.ValidationFailures, statusCode: statusCode, cancellation: default);
  }
}
