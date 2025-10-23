namespace Server.SharedKernel;

/// <summary>
/// Factory methods for creating custom Ardalis.Result instances.
/// These methods provide specialized error result creation without using reflection.
/// </summary>
public static class CustomArdalisResultFactory
{
  /// <summary>
  /// Represents a critical error that occurred during the execution of the service.
  /// Everything provided by the user was valid, but the service was unable to complete due to an exception.
  /// See also HTTP 500 Internal Server Error: https://en.wikipedia.org/wiki/List_of_HTTP_status_codes#5xx_server_errors
  /// </summary>
  /// <param name="exception">The exception that occurred</param>
  /// <returns>A Result&lt;T&gt;</returns>
  public static Ardalis.Result.Result<T> CriticalError<T>(Exception exception)
  {
    // Create validation error from the exception
    var validationError = new Ardalis.Result.ValidationError(exception.GetType().Name, exception.Message);

    // Create a CriticalError result with validation errors
    var result = Ardalis.Result.Result<T>.CriticalError();

    // Unfortunately, we still need reflection to set ValidationErrors since it has a protected setter
    // However, this is much simpler than the previous approach
    var validationErrorsProp = typeof(Ardalis.Result.Result<T>).GetProperty(nameof(Ardalis.Result.Result<T>.ValidationErrors))!;
    validationErrorsProp.SetValue(result, new[] { validationError });

    return result;
  }

  /// <summary>
  /// Represents a conflict that occurred during the execution of the service.
  /// The request could not be completed due to a conflict with the current state of the target resource.
  /// See also HTTP 409 Conflict: https://en.wikipedia.org/wiki/List_of_HTTP_status_codes#409
  /// </summary>
  /// <param name="exception">The exception that occurred</param>
  /// <returns>A Result&lt;T&gt;</returns>
  public static Ardalis.Result.Result<T> Conflict<T>(Exception exception)
  {
    // Create validation error from the exception
    var validationError = new Ardalis.Result.ValidationError(exception.GetType().Name, exception.Message);

    // Create a Conflict result with validation errors
    var result = Ardalis.Result.Result<T>.Conflict();

    // Unfortunately, we still need reflection to set ValidationErrors since it has a protected setter
    // However, this is much simpler than the previous approach
    var validationErrorsProp = typeof(Ardalis.Result.Result<T>).GetProperty(nameof(Ardalis.Result.Result<T>.ValidationErrors))!;
    validationErrorsProp.SetValue(result, new[] { validationError });

    return result;
  }
}
