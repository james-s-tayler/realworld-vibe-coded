using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Server.SharedKernel;

/// <summary>
/// MediatR pipeline behavior that catches exceptions during request handling
/// and transforms them into Result.CriticalError with ProblemDetails format.
/// This behavior only applies to requests that return IResult (Result or Result&lt;T&gt;).
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type, must implement IResult</typeparam>
public class ExceptionHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
  where TRequest : IRequest<TResponse>
  where TResponse : IResult
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

      return CreateResultFromException(ex, isConflict: true);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "An unhandled exception occurred while processing {RequestName}", typeof(TRequest).Name);
      return CreateResultFromException(ex, isConflict: false);
    }
  }

  private TResponse CreateResultFromException(Exception exception, bool isConflict)
  {
    var resultType = typeof(TResponse);

    // Check if this is a generic Result<T>
    if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Result<>))
    {
      // Use reflection to call the generic factory method
      var valueType = resultType.GetGenericArguments()[0];

      // Get the generic factory method
      var methodName = isConflict ? nameof(CustomArdalisResultFactory.Conflict) : nameof(CustomArdalisResultFactory.CriticalError);
      var method = typeof(CustomArdalisResultFactory)
        .GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
        .FirstOrDefault(m =>
          m.Name == methodName &&
          m.IsGenericMethodDefinition &&
          m.GetParameters().Length == 1 &&
          m.GetParameters()[0].ParameterType == typeof(Exception));

      if (method == null)
      {
        throw new InvalidOperationException($"Could not find generic factory method {methodName}<T>(Exception)");
      }

      var genericMethod = method.MakeGenericMethod(valueType);
      var result = genericMethod.Invoke(null, new object[] { exception });
      return (TResponse)result!;
    }
    // Check if this is a non-generic Result
    else if (resultType == typeof(Result))
    {
      // Call the non-generic factory method directly
      var result = isConflict
        ? CustomArdalisResultFactory.Conflict(exception)
        : CustomArdalisResultFactory.CriticalError(exception);
      return (TResponse)(object)result;
    }

    // This should never happen due to the IResult constraint
    throw new InvalidOperationException($"Response type {resultType.Name} is not a supported Result type");
  }
}
