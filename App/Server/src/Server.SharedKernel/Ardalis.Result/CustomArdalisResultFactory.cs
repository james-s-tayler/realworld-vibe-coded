namespace Server.SharedKernel.Ardalis.Result;

/// <summary>
/// Factory for creating Result instances with exceptions.
/// These methods are used by ExceptionHandlingBehavior.
/// </summary>
public static class CustomArdalisResultFactory
{
  /// <summary>
  /// Represents a critical error that occurred during the execution of the service.
  /// Everything provided by the user was valid, but the service was unable to complete due to an exception.
  /// See also HTTP 500 Internal Server Error: https://en.wikipedia.org/wiki/List_of_HTTP_status_codes#5xx_server_errors
  /// </summary>
  /// <param name="exception">The exception that occurred</param>
  /// <returns>A Result<typeparamref name="T"/></returns>
  public static Server.SharedKernel.Result.Result<T> CriticalError<T>(Exception exception)
  {
    return Server.SharedKernel.Result.Result<T>.CriticalError(exception);
  }

  /// <summary>
  /// Represents a conflict that occurred during the execution of the service.
  /// The request could not be completed due to a conflict with the current state of the target resource.
  /// See also HTTP 409 Conflict: https://en.wikipedia.org/wiki/List_of_HTTP_status_codes#409
  /// </summary>
  /// <param name="exception">The exception that occurred</param>
  /// <returns>A Result<typeparamref name="T"/></returns>
  public static Server.SharedKernel.Result.Result<T> Conflict<T>(Exception exception)
  {
    return Server.SharedKernel.Result.Result<T>.Conflict(exception);
  }
}
