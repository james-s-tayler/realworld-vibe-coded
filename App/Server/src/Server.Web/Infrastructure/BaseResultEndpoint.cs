using System.Security.Claims;
using Ardalis.Result;
using FastEndpoints;

namespace Server.Web.Infrastructure;

/// <summary>
/// Base endpoint class that provides standardized error and result handling
/// </summary>
/// <typeparam name="TRequest">Request type</typeparam>
/// <typeparam name="TResponse">Response type</typeparam>
public abstract class BaseResultEndpoint<TRequest, TResponse> : Endpoint<TRequest, TResponse>
  where TRequest : notnull, new()
{
  public override void OnValidationFailed()
  {
    HttpContext.Response.StatusCode = 422;
    HttpContext.Response.ContentType = "application/json";

    var json = ErrorResponseBuilder.CreateValidationErrorResponse(ValidationFailures);
    HttpContext.Response.WriteAsync(json).GetAwaiter().GetResult();
  }

  /// <summary>
  /// Helper method to get current user ID from claims
  /// </summary>
  protected int? GetCurrentUserId()
  {
    var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
    return userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId) ? userId : null;
  }

  /// <summary>
  /// Helper method to handle Result responses with appropriate HTTP status codes
  /// </summary>
  protected async Task HandleResultAsync<T>(Result<T> result, int successStatusCode = 200, CancellationToken cancellationToken = default)
  {
    if (result.IsSuccess)
    {
      HttpContext.Response.StatusCode = successStatusCode;
      if (typeof(TResponse).IsAssignableFrom(typeof(T)))
      {
        Response = (TResponse)(object)result.Value!;
      }
      return;
    }

    await HandleErrorResultAsync(result, cancellationToken);
  }

  /// <summary>
  /// Handle error results from Ardalis.Result
  /// </summary>
  private async Task HandleErrorResultAsync(Ardalis.Result.IResult result, CancellationToken cancellationToken)
  {
    HttpContext.Response.ContentType = "application/json";

    switch (result.Status)
    {
      case ResultStatus.Invalid:
        HttpContext.Response.StatusCode = 422;
        var validationResponse = ErrorResponseBuilder.CreateValidationErrorResponse(result.ValidationErrors);
        await HttpContext.Response.WriteAsync(validationResponse, cancellationToken);
        break;

      case ResultStatus.Unauthorized:
        HttpContext.Response.StatusCode = 401;
        var unauthorizedResponse = ErrorResponseBuilder.CreateUnauthorizedResponse();
        await HttpContext.Response.WriteAsync(unauthorizedResponse, cancellationToken);
        break;

      case ResultStatus.Forbidden:
        HttpContext.Response.StatusCode = 403;
        var forbiddenResponse = ErrorResponseBuilder.CreateErrorResponse("Forbidden");
        await HttpContext.Response.WriteAsync(forbiddenResponse, cancellationToken);
        break;

      case ResultStatus.NotFound:
        HttpContext.Response.StatusCode = 404;
        var notFoundResponse = ErrorResponseBuilder.CreateErrorResponse("Not found");
        await HttpContext.Response.WriteAsync(notFoundResponse, cancellationToken);
        break;

      case ResultStatus.Conflict:
        HttpContext.Response.StatusCode = 409;
        var conflictResponse = ErrorResponseBuilder.CreateErrorResponse(result.Errors);
        await HttpContext.Response.WriteAsync(conflictResponse, cancellationToken);
        break;

      case ResultStatus.Error:
      default:
        HttpContext.Response.StatusCode = 422; // Use 422 for general errors to match RealWorld API expectations
        var errorResponse = ErrorResponseBuilder.CreateErrorResponse(result.Errors);
        await HttpContext.Response.WriteAsync(errorResponse, cancellationToken);
        break;
    }
  }
}
