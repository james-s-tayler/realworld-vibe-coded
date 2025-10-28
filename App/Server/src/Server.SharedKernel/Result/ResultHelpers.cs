namespace Server.SharedKernel.Result;

/// <summary>
/// Static helper class for creating Result{T} instances.
/// Provides convenience methods for creating results without specifying the type parameter.
/// </summary>
public static class Result
{
  /// <summary>
  /// Represents a successful operation and accepts a values as the result of the operation
  /// </summary>
  /// <param name="value">Sets the Value property</param>
  /// <returns>A Result{typeparamref name="T"/}</returns>
  public static Result<T> Success<T>(T value) => Result<T>.Success(value);

  /// <summary>
  /// Represents a successful operation that resulted in the creation of a new resource.
  /// </summary>
  /// <param name="value">The value of the resource created.</param>
  /// <returns>A Result{typeparamref name="T"/> with status Created.</returns>
  public static Result<T> Created<T>(T value) => Result<T>.Created(value);

  /// <summary>
  /// Represents the situation where a service was unable to find a requested resource.
  /// </summary>
  /// <param name="errorMessage">An error message that will be converted to a validation error.</param>
  /// <returns>A Result{typeparamref name="T"/}</returns>
  public static Result<T> NotFound<T>(string errorMessage) => Result<T>.NotFound(errorMessage);

  /// <summary>
  /// The parameters to the call were correct, but the user does not have permission to perform some action.
  /// </summary>
  /// <param name="errorMessage">An error message that will be converted to a validation error.</param>
  /// <returns>A Result{typeparamref name="T"/}</returns>
  public static Result<T> Forbidden<T>(string errorMessage) => Result<T>.Forbidden(errorMessage);

  /// <summary>
  /// This is similar to Forbidden, but should be used when the user has not authenticated or has attempted to authenticate but failed.
  /// </summary>
  /// <returns>A Result{typeparamref name="T"/}</returns>
  public static Result<T> Unauthorized<T>() => Result<T>.Unauthorized();

  /// <summary>
  /// This is similar to Forbidden, but should be used when the user has not authenticated or has attempted to authenticate but failed.
  /// </summary>
  /// <param name="errorMessage">An error message that will be converted to a validation error.</param>
  /// <returns>A Result{typeparamref name="T"/}</returns>
  public static Result<T> Unauthorized<T>(string errorMessage) => Result<T>.Unauthorized(errorMessage);

  /// <summary>
  /// Represents an error that occurred during the execution of the service.
  /// </summary>
  /// <param name="errorMessage">An error message that will be converted to a validation error.</param>
  /// <returns>A Result{typeparamref name="T"/}</returns>
  public static Result<T> Error<T>(string errorMessage) => Result<T>.Error(errorMessage);

  /// <summary>
  /// Represents a validation error that prevents the underlying service from completing.
  /// </summary>
  /// <param name="validationError">The validation error encountered</param>
  /// <returns>A Result{typeparamref name="T"/}</returns>
  public static Result<T> Invalid<T>(ValidationError validationError) => Result<T>.Invalid(validationError);

  /// <summary>
  /// Represents validation errors that prevent the underlying service from completing.
  /// </summary>
  /// <param name="validationErrors">A list of validation errors encountered</param>
  /// <returns>A Result{typeparamref name="T"/}</returns>
  public static Result<T> Invalid<T>(params ValidationError[] validationErrors) => Result<T>.Invalid(validationErrors);
}
