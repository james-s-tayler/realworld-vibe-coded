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
      return CreateResultFromException(ex, nameof(CustomArdalisResultFactory.Conflict));
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "An unhandled exception occurred while processing {RequestName}", typeof(TRequest).Name);
      return CreateResultFromException(ex, nameof(CustomArdalisResultFactory.CriticalError));
    }
  }

  private TResponse CreateResultFromException(Exception exception, string factoryMethodName)
  {
    var resultType = typeof(TResponse);
    var helperType = typeof(CustomArdalisResultFactory);

    // Check if this is a generic Result<T>
    if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Ardalis.Result.Result<>))
    {
      // Use reflection to call the generic factory method
      var valueType = resultType.GetGenericArguments()[0];

      // Get all methods with the specified name
      var methods = helperType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
        .Where(m => m.Name == factoryMethodName && m.IsGenericMethodDefinition)
        .ToArray();

      if (methods.Length > 0)
      {
        var genericMethod = methods[0].MakeGenericMethod(valueType);
        var result = genericMethod.Invoke(null, new object[] { exception });
        return (TResponse)result!;
      }
    }
    // Check if this is a non-generic Result
    else if (resultType == typeof(Ardalis.Result.Result))
    {
      // Get all non-generic methods with the specified name
      var methods = helperType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
        .Where(m => m.Name == factoryMethodName && !m.IsGenericMethodDefinition && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(Exception))
        .ToArray();

      if (methods.Length > 0)
      {
        var result = methods[0].Invoke(null, new object[] { exception });
        return (TResponse)result!;
      }
    }

    // If not a Result type or factory method not found, rethrow the exception
    throw exception;
  }
}
