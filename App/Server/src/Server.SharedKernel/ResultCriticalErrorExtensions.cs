namespace Ardalis.Result;

/// <summary>
/// Extensions to Ardalis.Result.Result to support custom CriticalError overloads with ValidationError.
/// These methods are used by ExceptionHandlingBehavior via reflection.
/// </summary>
public static class ResultCriticalErrorExtensions
{
  /// <summary>
  /// Represents a critical error that occurred during the execution of the service.
  /// Everything provided by the user was valid, but the service was unable to complete due to an exception.
  /// Validation error may be provided and will be exposed via the ValidationErrors property.
  /// See also HTTP 500 Internal Server Error: https://en.wikipedia.org/wiki/List_of_HTTP_status_codes#5xx_server_errors
  /// </summary>
  /// <param name="validationError">The validation error encountered</param>
  /// <returns>A Result<typeparamref name="T"/></returns>
  public static Result<T> CriticalError<T>(ValidationError validationError)
  {
    // Use reflection to create Result<T> with CriticalError status and set ValidationErrors
    var result = (Result<T>)Activator.CreateInstance(
      typeof(Result<T>),
      System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
      null,
      new object[] { ResultStatus.CriticalError },
      null
    )!;

    // Set ValidationErrors via reflection since it has a protected setter
    var validationErrorsProp = typeof(Result<T>).GetProperty(nameof(Result<T>.ValidationErrors))!;
    validationErrorsProp.SetValue(result, new[] { validationError });

    return result;
  }

  /// <summary>
  /// Represents a critical error that occurred during the execution of the service.
  /// Everything provided by the user was valid, but the service was unable to complete due to an exception.
  /// Validation errors may be provided and will be exposed via the ValidationErrors property.
  /// See also HTTP 500 Internal Server Error: https://en.wikipedia.org/wiki/List_of_HTTP_status_codes#5xx_server_errors
  /// </summary>
  /// <param name="validationErrors">A list of validation errors encountered</param>
  /// <returns>A Result<typeparamref name="T"/></returns>
  public static Result<T> CriticalError<T>(ValidationError[] validationErrors)
  {
    // Use reflection to create Result<T> with CriticalError status and set ValidationErrors
    var result = (Result<T>)Activator.CreateInstance(
      typeof(Result<T>),
      System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
      null,
      new object[] { ResultStatus.CriticalError },
      null
    )!;

    // Set ValidationErrors via reflection since it has a protected setter
    var validationErrorsProp = typeof(Result<T>).GetProperty(nameof(Result<T>.ValidationErrors))!;
    validationErrorsProp.SetValue(result, validationErrors);

    return result;
  }
}
