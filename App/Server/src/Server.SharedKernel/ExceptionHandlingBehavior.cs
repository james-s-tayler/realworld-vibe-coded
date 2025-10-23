using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Server.SharedKernel;

/// <summary>
/// MediatR pipeline behavior that catches exceptions during request handling
/// and transforms them into Result.CriticalError with ProblemDetails format.
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

      // Only handle Result<T> types, rethrow for non-Result types
      var resultType = typeof(TResponse);
      if (!resultType.IsGenericType || resultType.GetGenericTypeDefinition() != typeof(Ardalis.Result.Result<>))
      {
        throw;
      }

      // Create a Conflict result for concurrency exceptions
      var valueType = resultType.GetGenericArguments()[0];
      var helperType = typeof(CustomArdalisResultFactory);
      var conflictMethod = helperType.GetMethod(nameof(CustomArdalisResultFactory.Conflict), new[] { typeof(Exception) });

      if (conflictMethod != null)
      {
        var genericMethod = conflictMethod.MakeGenericMethod(valueType);
        var result = genericMethod.Invoke(null, new object[] { ex });
        return (TResponse)result!;
      }

      // Fallback for non-generic Result types
      throw;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "An unhandled exception occurred while processing {RequestName}", typeof(TRequest).Name);

      // Only handle Result<T> types, rethrow for non-Result types
      var resultType = typeof(TResponse);
      if (!resultType.IsGenericType || resultType.GetGenericTypeDefinition() != typeof(Ardalis.Result.Result<>))
      {
        throw;
      }

      // Create a CriticalError result
      var valueType = resultType.GetGenericArguments()[0];
      var helperType = typeof(CustomArdalisResultFactory);
      var criticalErrorMethod = helperType.GetMethod(nameof(CustomArdalisResultFactory.CriticalError), new[] { typeof(Exception) });

      if (criticalErrorMethod != null)
      {
        var genericMethod = criticalErrorMethod.MakeGenericMethod(valueType);
        var result = genericMethod.Invoke(null, new object[] { ex });
        return (TResponse)result!;
      }

      // Fallback for non-generic Result types
      throw;
    }
  }
}
