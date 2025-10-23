using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Server.SharedKernel;

/// <summary>
/// MediatR pipeline behavior that catches exceptions during request handling
/// and transforms them into Result.CriticalError with ProblemDetails format.
/// This implementation uses a wrapper approach to avoid runtime reflection in the pipeline.
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type, must be Result<T></typeparam>
public class ExceptionHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
  where TRequest : IRequest<TResponse>
{
  private readonly ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> _logger;
  private readonly IExceptionResultFactory<TResponse> _resultFactory;

  public ExceptionHandlingBehavior(
    ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> logger,
    IExceptionResultFactory<TResponse> resultFactory)
  {
    _logger = logger;
    _resultFactory = resultFactory;
  }

  public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
  {
    try
    {
      return await next();
    }
    catch (DbUpdateConcurrencyException ex)
    {
      _logger.LogWarning(ex, "Concurrency conflict occurred while processing {RequestName}", typeof(TRequest).Name);

      return _resultFactory.CreateConflict(ex);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "An unhandled exception occurred while processing {RequestName}", typeof(TRequest).Name);
      return _resultFactory.CreateCriticalError(ex);
    }
  }
}

/// <summary>
/// Factory interface for creating Result instances from exceptions without reflection.
/// TResult should be the complete Result<T> type.
/// </summary>
public interface IExceptionResultFactory<out TResult>
{
  TResult CreateCriticalError(Exception exception);
  TResult CreateConflict(Exception exception);
}

/// <summary>
/// Base implementation of IExceptionResultFactory that uses reflection during factory creation (not in hot path).
/// This is instantiated once per Result<T> type via DI.
/// The reflection happens once during construction, not on every exception.
/// </summary>
public class ExceptionResultFactory<TResult> : IExceptionResultFactory<TResult>
{
  private readonly Func<Exception, TResult>? _criticalErrorFactory;
  private readonly Func<Exception, TResult>? _conflictFactory;

  public ExceptionResultFactory()
  {
    var resultType = typeof(TResult);

    // Check if TResult is Result<T> and build the factory delegates once during construction
    if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Result<>))
    {
      var valueType = resultType.GetGenericArguments()[0];

      // Create delegates that call the CustomArdalisResultFactory methods
      // This reflection happens once per TResult type at construction, not on every call
      var criticalErrorMethod = typeof(CustomArdalisResultFactory)
        .GetMethod(nameof(CustomArdalisResultFactory.CriticalError))
        ?.MakeGenericMethod(valueType);

      var conflictMethod = typeof(CustomArdalisResultFactory)
        .GetMethod(nameof(CustomArdalisResultFactory.Conflict))
        ?.MakeGenericMethod(valueType);

      if (criticalErrorMethod != null)
      {
        _criticalErrorFactory = (ex) => (TResult)criticalErrorMethod.Invoke(null, new object[] { ex })!;
      }

      if (conflictMethod != null)
      {
        _conflictFactory = (ex) => (TResult)conflictMethod.Invoke(null, new object[] { ex })!;
      }
    }
  }

  public TResult CreateCriticalError(Exception exception)
  {
    if (_criticalErrorFactory == null)
    {
      throw new InvalidOperationException($"Cannot create CriticalError for type {typeof(TResult).Name}");
    }
    return _criticalErrorFactory(exception);
  }

  public TResult CreateConflict(Exception exception)
  {
    if (_conflictFactory == null)
    {
      throw new InvalidOperationException($"Cannot create Conflict for type {typeof(TResult).Name}");
    }
    return _conflictFactory(exception);
  }
}
