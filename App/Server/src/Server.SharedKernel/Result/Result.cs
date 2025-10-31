using System.Text.Json.Serialization;

namespace Server.SharedKernel.Result;

public class Result<T>
{
  protected Result() { }

  public Result(T value) => Value = value;

  protected internal Result(T value, string successMessage) : this(value) => SuccessMessage = successMessage;

  protected internal Result(ResultStatus status) => Status = status;

  public static implicit operator T(Result<T> result) => result.Value;
  public static implicit operator Result<T>(T value) => new Result<T>(value);

  [JsonInclude]
  public T Value { get; init; } = default!;

  [JsonIgnore]
  public Type ValueType => typeof(T);

  [JsonInclude]
  public ResultStatus Status { get; protected set; } = ResultStatus.Ok;

  public bool IsSuccess => Status is ResultStatus.Ok or ResultStatus.NoContent or ResultStatus.Created;

  [JsonInclude]
  public string SuccessMessage { get; protected set; } = string.Empty;

  [JsonInclude]
  public string CorrelationId { get; protected set; } = string.Empty;

  [JsonInclude]
  public string Location { get; protected set; } = string.Empty;

  [JsonInclude]
  public IEnumerable<ErrorDetail> ErrorDetails { get; protected set; } = [];

  /// <summary>
  /// Returns the current value.
  /// </summary>
  /// <returns></returns>
  public object? GetValue() => this.Value;

  /// <summary>
  /// Represents a successful operation and accepts a values as the result of the operation
  /// </summary>
  /// <param name="value">Sets the Value property</param>
  /// <returns>A Result<typeparamref name="T"/></returns>
  public static Result<T> Success(T value) => new(value);

  /// <summary>
  /// Represents a successful operation and accepts a values as the result of the operation
  /// Sets the SuccessMessage property to the provided value
  /// </summary>
  /// <param name="value">Sets the Value property</param>
  /// <param name="successMessage">Sets the SuccessMessage property</param>
  /// <returns>A Result<typeparamref name="T"/></returns>
  public static Result<T> Success(T value, string successMessage) => new(value, successMessage);

  /// <summary>
  /// Represents a successful operation that resulted in the creation of a new resource.
  /// </summary>
  /// <typeparam name="T">The type of the resource created.</typeparam>
  /// <returns>A Result<typeparamref name="T"/> with status Created.</returns>
  public static Result<T> Created(T value) => new(ResultStatus.Created) { Value = value };

  /// <summary>
  /// Represents a successful operation that resulted in the creation of a new resource.
  /// Sets the SuccessMessage property to the provided value.
  /// </summary>
  /// <typeparam name="T">The type of the resource created.</typeparam>
  /// <param name="value">The value of the resource created.</param>
  /// <param name="location">The URL indicating where the newly created resource can be accessed.</param>
  /// <returns>A Result<typeparamref name="T"/> with status Created.</returns>
  public static Result<T> Created(T value, string location) => new(ResultStatus.Created) { Value = value, Location = location };

  /// <summary>
  /// Represents a validation error that prevents the underlying service from completing.
  /// </summary>
  /// <param name="validationError">The validation error encountered</param>
  /// <returns>A Result<typeparamref name="T"/></returns>
  public static Result<T> Invalid(ErrorDetail validationError)
      => new(ResultStatus.Invalid) { ErrorDetails = [validationError] };

  /// <summary>
  /// Represents validation errors that prevent the underlying service from completing.
  /// </summary>
  /// <param name="errorDetails">A list of validation errors encountered</param>
  /// <returns>A Result<typeparamref name="T"/></returns>
  public static Result<T> Invalid(params ErrorDetail[] errorDetails) =>
      new(ResultStatus.Invalid)
      { ErrorDetails = new List<ErrorDetail>(errorDetails) };

  /// <summary>
  /// Represents validation errors that prevent the underlying service from completing.
  /// </summary>
  /// <param name="errorDetails">A list of validation errors encountered</param>
  /// <returns>A Result<typeparamref name="T"/></returns>
  public static Result<T> Invalid(IEnumerable<ErrorDetail> errorDetails)
      => new(ResultStatus.Invalid) { ErrorDetails = errorDetails };

  /// <summary>
  /// Represents the situation where a service was unable to find a requested resource.
  /// </summary>
  /// <returns>A Result<typeparamref name="T"/></returns>
  public static Result<T> NotFound() => new(ResultStatus.NotFound);

  /// <summary>
  /// Represents the situation where a service was unable to find a requested resource identified by a Guid.
  /// </summary>
  /// <param name="id">The Guid identifier of the resource that was not found.</param>
  /// <returns>A Result<typeparamref name="T"/></returns>
  public static Result<T> NotFound(Guid id) =>
    NotFound(new ErrorDetail($"{typeof(T).Name}", $"{typeof(T).Name} identified by {id} was not found"));

  /// <summary>
  /// Represents the situation where a service was unable to find a requested resource identified by a string.
  /// </summary>
  /// <param name="identifier">The string identifier of the resource that was not found.</param>
  /// <returns>A Result<typeparamref name="T"/></returns>
  public static Result<T> NotFound(string identifier) =>
    NotFound(new ErrorDetail($"{typeof(T).Name}", $"{typeof(T).Name} identified by {identifier} was not found"));

  /// <summary>
  /// Represents the situation where a service was unable to find a requested resource of a specific type identified by a Guid.
  /// </summary>
  /// <param name="entityType">The type of the resource that was not found.</param>
  /// <param name="id">The Guid identifier of the resource that was not found.</param>
  /// <returns>A Result<typeparamref name="T"/></returns>
  public static Result<T> NotFound(Type entityType, Guid id) =>
    NotFound(new ErrorDetail(entityType.Name, $"{entityType.Name} identified by {id} was not found"));

  /// <summary>
  /// Represents the situation where a service was unable to find a requested resource of a specific type identified by a string.
  /// </summary>
  /// <param name="entityType">The type of the resource that was not found.</param>
  /// <param name="identifier">The string identifier of the resource that was not found.</param>
  /// <returns>A Result<typeparamref name="T"/></returns>
  public static Result<T> NotFound(Type entityType, string identifier) =>
    NotFound(new ErrorDetail(entityType.Name, $"{entityType.Name} identified by {identifier} was not found"));

  /// <summary>
  /// Represents the situation where a service was unable to find a requested resource.
  /// Error details may be provided and will be exposed via the ErrorDetails property.
  /// </summary>
  /// <param name="errorDetails">A list of error details.</param>
  /// <returns>A Result<typeparamref name="T"/></returns>
  private static Result<T> NotFound(params ErrorDetail[] errorDetails) =>
    new(ResultStatus.NotFound) { ErrorDetails = errorDetails };

  /// <summary>
  /// The parameters to the call were correct, but the user does not have permission to perform some action.
  /// See also HTTP 403 Forbidden: https://en.wikipedia.org/wiki/List_of_HTTP_status_codes#4xx_client_errors
  /// </summary>
  /// <returns>A Result<typeparamref name="T"/></returns>
  public static Result<T> Forbidden() => new(ResultStatus.Forbidden);

  /// <summary>
  /// The parameters to the call were correct, but the user does not have permission to perform some action.
  /// See also HTTP 403 Forbidden: https://en.wikipedia.org/wiki/List_of_HTTP_status_codes#4xx_client_errors
  /// </summary>
  /// <param name="errorDetails">A list of error details.</param> 
  /// <returns>A Result<typeparamref name="T"/></returns>
  public static Result<T> Forbidden(params ErrorDetail[] errorDetails) =>
    new(ResultStatus.Forbidden) { ErrorDetails = errorDetails };

  /// <summary>
  /// This is similar to Forbidden, but should be used when the user has not authenticated or has attempted to authenticate but failed.
  /// See also HTTP 401 Unauthorized: https://en.wikipedia.org/wiki/List_of_HTTP_status_codes#4xx_client_errors
  /// </summary>
  /// <returns>A Result<typeparamref name="T"/></returns>
  public static Result<T> Unauthorized() => new(ResultStatus.Unauthorized);

  /// <summary>
  /// This is similar to Forbidden, but should be used when the user has not authenticated or has attempted to authenticate but failed.
  /// See also HTTP 401 Unauthorized: https://en.wikipedia.org/wiki/List_of_HTTP_status_codes#4xx_client_errors
  /// </summary>
  /// <param name="errorDetails">A list of error details.</param>  
  /// <returns>A Result<typeparamref name="T"/></returns>
  public static Result<T> Unauthorized(params ErrorDetail[] errorDetails) =>
    new(ResultStatus.Unauthorized) { ErrorDetails = errorDetails };

  /// <summary>
  /// Represents an error that occurred during the execution of the service.
  /// </summary>
  /// <param name="errorDetails">A list of error details.</param>
  /// <returns>A Result<typeparamref name="T"/></returns>
  public static Result<T> Error(params ErrorDetail[] errorDetails) =>
    new(ResultStatus.Error) { ErrorDetails = errorDetails };

  /// <summary>
  /// Represents an error where a required entity for processing is missing.
  /// </summary>
  /// <param name="entityType">The type of the required entity that is missing.</param>
  /// <returns>A Result<typeparamref name="T"/></returns>
  public static Result<T> ErrorMissingRequiredEntity(Type entityType) =>
    Error(new ErrorDetail(entityType.Name, $"{entityType.Name} is required for processing, but is missing."));

  /// <summary>
  /// Represents an error where a required entity for processing is missing, identified by a Guid.
  /// </summary>
  /// <param name="entityType">The type of the required entity that is missing.</param>
  /// <param name="id">The Guid identifier of the missing entity.</param>
  /// <returns>A Result<typeparamref name="T"/></returns>
  public static Result<T> ErrorMissingRequiredEntity(Type entityType, Guid id) =>
    Error(new ErrorDetail(entityType.Name, $"{entityType.Name} identified by {id} is required for processing, but is missing."));

  /// <summary>
  /// Represents an error where a required entity for processing is missing, identified by a string.
  /// </summary>
  /// <param name="entityType">The type of the required entity that is missing.</param>
  /// <param name="identifier">The string identifier of the missing entity.</param>
  /// <returns>A Result<typeparamref name="T"/></returns>
  public static Result<T> ErrorMissingRequiredEntity(Type entityType, string identifier) =>
    Error(new ErrorDetail(entityType.Name, $"{entityType.Name} identified by {identifier} is required for processing, but is missing."));

  /// <summary>
  /// Represents a situation where a service is in conflict due to the current state of a resource,
  /// such as an edit conflict between multiple concurrent updates.
  /// See also HTTP 409 Conflict: https://en.wikipedia.org/wiki/List_of_HTTP_status_codes#4xx_client_errors
  /// </summary>
  /// <returns>A Result<typeparamref name="T"/></returns>
  public static Result<T> Conflict() => new(ResultStatus.Conflict);

  /// <summary>
  /// Represents a situation where a service is in conflict due to the current state of a resource,
  /// such as an edit conflict between multiple concurrent updates.
  /// Validation errors may be provided and will be exposed via the ErrorDetails property.
  /// See also HTTP 409 Conflict: https://en.wikipedia.org/wiki/List_of_HTTP_status_codes#4xx_client_errors
  /// </summary>
  /// <param name="errorDetails">A list of validation errors.</param>
  /// <returns>A Result<typeparamref name="T"/></returns>
  public static Result<T> Conflict(params ErrorDetail[] errorDetails) =>
    new(ResultStatus.Conflict) { ErrorDetails = errorDetails };

  /// <summary>
  /// Represents a situation where a service is in conflict due to the current state of a resource,
  /// such as an edit conflict between multiple concurrent updates.
  /// Validation errors may be provided and will be exposed via the ErrorDetails property.
  /// See also HTTP 409 Conflict: https://en.wikipedia.org/wiki/List_of_HTTP_status_codes#4xx_client_errors
  /// </summary>
  /// <param name="errorDetails">A list of validation errors.</param>
  /// <returns>A Result<typeparamref name="T"/></returns>
  public static Result<T> Conflict(IEnumerable<ErrorDetail> errorDetails) =>
    new(ResultStatus.Conflict) { ErrorDetails = errorDetails };

  /// <summary>
  /// Represents a situation where a service is in conflict due to the current state of a resource.
  /// Creates a conflict result from an exception.
  /// See also HTTP 409 Conflict: https://en.wikipedia.org/wiki/List_of_HTTP_status_codes#4xx_client_errors
  /// </summary>
  /// <param name="exception">The exception that occurred</param>
  /// <returns>A Result<typeparamref name="T"/></returns>
  public static Result<T> Conflict(Exception exception) =>
    new(ResultStatus.Conflict)
    {
      ErrorDetails = [new ErrorDetail(exception.GetType().Name, exception.Message)]
    };

  /// <summary>
  /// Represents a critical error that occurred during the execution of the service.
  /// Everything provided by the user was valid, but the service was unable to complete due to an exception.
  /// See also HTTP 500 Internal Server Error: https://en.wikipedia.org/wiki/List_of_HTTP_status_codes#5xx_server_errors
  /// </summary>
  /// <param name="errorDetails">A list of validation errors.</param>
  /// <returns>A Result<typeparamref name="T"/></returns>
  public static Result<T> CriticalError(params ErrorDetail[] errorDetails) =>
    new(ResultStatus.CriticalError) { ErrorDetails = errorDetails };

  /// <summary>
  /// Represents a critical error that occurred during the execution of the service.
  /// Everything provided by the user was valid, but the service was unable to complete due to an exception.
  /// See also HTTP 500 Internal Server Error: https://en.wikipedia.org/wiki/List_of_HTTP_status_codes#5xx_server_errors
  /// </summary>
  /// <param name="errorDetails">A list of validation errors.</param>
  /// <returns>A Result<typeparamref name="T"/></returns>
  public static Result<T> CriticalError(IEnumerable<ErrorDetail> errorDetails) =>
    new(ResultStatus.CriticalError) { ErrorDetails = errorDetails };

  /// <summary>
  /// Represents a critical error that occurred during the execution of the service.
  /// Everything provided by the user was valid, but the service was unable to complete due to an exception.
  /// Creates a critical error result from an exception.
  /// See also HTTP 500 Internal Server Error: https://en.wikipedia.org/wiki/List_of_HTTP_status_codes#5xx_server_errors
  /// </summary>
  /// <param name="exception">The exception that occurred</param>
  /// <returns>A Result<typeparamref name="T"/></returns>
  public static Result<T> CriticalError(Exception exception) =>
    new(ResultStatus.CriticalError)
    {
      ErrorDetails = [new ErrorDetail(exception.GetType().Name, exception.Message)]
    };

  /// <summary>
  /// Represents a situation where a service is unavailable, such as when the underlying data store is unavailable.
  /// Errors may be transient, so the caller may wish to retry the operation.
  /// See also HTTP 503 Service Unavailable: https://en.wikipedia.org/wiki/List_of_HTTP_status_codes#5xx_server_errors
  /// </summary>
  /// <param name="errorDetails">A list of validation errors</param>
  /// <returns></returns>
  public static Result<T> Unavailable(params ErrorDetail[] errorDetails) =>
    new(ResultStatus.Unavailable) { ErrorDetails = errorDetails };

  /// <summary>
  /// Represents a situation where the server has successfully fulfilled the request, but there is no content to send back in the response body.
  /// </summary>
  /// <typeparam name="T">The type parameter representing the expected response data.</typeparam>
  /// <returns>A Result object</returns>
  public static Result<T> NoContent() => new(ResultStatus.NoContent);
}
