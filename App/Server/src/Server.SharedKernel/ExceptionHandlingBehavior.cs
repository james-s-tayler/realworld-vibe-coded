using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Server.SharedKernel;

/// <summary>
/// MediatR pipeline behavior that catches exceptions during request handling
/// and transforms them into Result.CriticalError or Result.Conflict with ProblemDetails format.
/// Works with any IRequest that returns a Result&lt;T&gt;.
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class ExceptionHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
  where TRequest : IRequest<TResponse>
{
  private readonly ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> _logger;

  public ExceptionHandlingBehavior(ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> logger)
  {
    _logger = logger;
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
      return CreateConflictResult(ex);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "An unhandled exception occurred while processing {RequestName}", typeof(TRequest).Name);
      return CreateCriticalErrorResult(ex);
    }
  }

  private TResponse CreateConflictResult(Exception exception)
  {
    var responseType = typeof(TResponse);

    // Check if this is a generic Result<T>
    if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
    {
      var valueType = responseType.GetGenericArguments()[0];
      var method = typeof(CustomArdalisResultFactory).GetMethod(nameof(CustomArdalisResultFactory.Conflict))!;
      var genericMethod = method.MakeGenericMethod(valueType);
      return (TResponse)genericMethod.Invoke(null, new object[] { exception })!;
    }

    // If not a Result type, rethrow the exception
    throw exception;
  }

  private TResponse CreateCriticalErrorResult(Exception exception)
  {
    var responseType = typeof(TResponse);

    // Check if this is a generic Result<T>
    if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
    {
      var valueType = responseType.GetGenericArguments()[0];
      var method = typeof(CustomArdalisResultFactory).GetMethod(nameof(CustomArdalisResultFactory.CriticalError))!;
      var genericMethod = method.MakeGenericMethod(valueType);
      return (TResponse)genericMethod.Invoke(null, new object[] { exception })!;
    }

    // If not a Result type, rethrow the exception
    throw exception;
  }
}
