namespace Server.SharedKernel;

/// <summary>
/// Extensions to Ardalis.Result.Result to support custom CriticalError overloads with ValidationError.
/// These methods are used by ExceptionHandlingBehavior via reflection.
/// </summary>
public static class CustomArdalisResultFactory
{
  /// <summary>
  /// Represents a critical error that occurred during the execution of the service.
  /// Everything provided by the user was valid, but the service was unable to complete due to an exception.
  /// Validation error may be provided and will be exposed via the ValidationErrors property.
  /// See also HTTP 500 Internal Server Error: https://en.wikipedia.org/wiki/List_of_HTTP_status_codes#5xx_server_errors
  /// </summary>
  /// <param name="validationError">The validation error encountered</param>
  /// <returns>A Result<typeparamref name="T"/></returns>
  /// <exception cref="InvalidOperationException">Thrown if reflection fails to access Ardalis.Result internals</exception>
  public static Ardalis.Result.Result<T> CriticalError<T>(Ardalis.Result.ValidationError validationError)
  {
    try
    {
      // Use reflection to create Result<T> with CriticalError status and set ValidationErrors
      var resultInstance = Activator.CreateInstance(
        typeof(Ardalis.Result.Result<T>),
        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
        null,
        new object[] { Ardalis.Result.ResultStatus.CriticalError },
        null
      );

      if (resultInstance == null)
      {
        throw new InvalidOperationException(
          $"Failed to create instance of Ardalis.Result.Result<{typeof(T).Name}> via reflection. " +
          "The protected constructor with ResultStatus parameter may have been removed or modified in the Ardalis.Result library.");
      }

      var result = (Ardalis.Result.Result<T>)resultInstance;

      // Set ValidationErrors via reflection since it has a protected setter
      var validationErrorsProp = typeof(Ardalis.Result.Result<T>).GetProperty(nameof(Ardalis.Result.Result<T>.ValidationErrors));
      if (validationErrorsProp == null)
      {
        throw new InvalidOperationException(
          $"Failed to find ValidationErrors property on Ardalis.Result.Result<{typeof(T).Name}>. " +
          "The property may have been renamed or removed in the Ardalis.Result library.");
      }

      var setter = validationErrorsProp.GetSetMethod(nonPublic: true);
      if (setter == null)
      {
        throw new InvalidOperationException(
          $"Failed to find setter for ValidationErrors property on Ardalis.Result.Result<{typeof(T).Name}>. " +
          "The property setter may have been removed or made inaccessible in the Ardalis.Result library.");
      }

      validationErrorsProp.SetValue(result, new[] { validationError });

      return result;
    }
    catch (Exception ex) when (ex is not InvalidOperationException)
    {
      throw new InvalidOperationException(
        $"Reflection-based creation of Ardalis.Result.Result<{typeof(T).Name}> failed. " +
        "This may indicate an incompatible version of the Ardalis.Result library. " +
        $"Inner exception: {ex.GetType().Name}: {ex.Message}",
        ex);
    }
  }

  /// <summary>
  /// Represents a critical error that occurred during the execution of the service.
  /// Everything provided by the user was valid, but the service was unable to complete due to an exception.
  /// Validation errors may be provided and will be exposed via the ValidationErrors property.
  /// See also HTTP 500 Internal Server Error: https://en.wikipedia.org/wiki/List_of_HTTP_status_codes#5xx_server_errors
  /// </summary>
  /// <param name="validationErrors">A list of validation errors encountered</param>
  /// <returns>A Result<typeparamref name="T"/></returns>
  /// <exception cref="InvalidOperationException">Thrown if reflection fails to access Ardalis.Result internals</exception>
  public static Ardalis.Result.Result<T> CriticalError<T>(Ardalis.Result.ValidationError[] validationErrors)
  {
    try
    {
      // Use reflection to create Result<T> with CriticalError status and set ValidationErrors
      var resultInstance = Activator.CreateInstance(
        typeof(Ardalis.Result.Result<T>),
        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
        null,
        new object[] { Ardalis.Result.ResultStatus.CriticalError },
        null
      );

      if (resultInstance == null)
      {
        throw new InvalidOperationException(
          $"Failed to create instance of Ardalis.Result.Result<{typeof(T).Name}> via reflection. " +
          "The protected constructor with ResultStatus parameter may have been removed or modified in the Ardalis.Result library.");
      }

      var result = (Ardalis.Result.Result<T>)resultInstance;

      // Set ValidationErrors via reflection since it has a protected setter
      var validationErrorsProp = typeof(Ardalis.Result.Result<T>).GetProperty(nameof(Ardalis.Result.Result<T>.ValidationErrors));
      if (validationErrorsProp == null)
      {
        throw new InvalidOperationException(
          $"Failed to find ValidationErrors property on Ardalis.Result.Result<{typeof(T).Name}>. " +
          "The property may have been renamed or removed in the Ardalis.Result library.");
      }

      var setter = validationErrorsProp.GetSetMethod(nonPublic: true);
      if (setter == null)
      {
        throw new InvalidOperationException(
          $"Failed to find setter for ValidationErrors property on Ardalis.Result.Result<{typeof(T).Name}>. " +
          "The property setter may have been removed or made inaccessible in the Ardalis.Result library.");
      }

      validationErrorsProp.SetValue(result, validationErrors);

      return result;
    }
    catch (Exception ex) when (ex is not InvalidOperationException)
    {
      throw new InvalidOperationException(
        $"Reflection-based creation of Ardalis.Result.Result<{typeof(T).Name}> failed. " +
        "This may indicate an incompatible version of the Ardalis.Result library. " +
        $"Inner exception: {ex.GetType().Name}: {ex.Message}",
        ex);
    }
  }
}
