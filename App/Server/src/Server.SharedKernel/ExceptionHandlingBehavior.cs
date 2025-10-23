using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Server.SharedKernel;

/// <summary>
/// MediatR pipeline behavior that catches exceptions during request handling
/// and transforms them into Result.CriticalError with ProblemDetails format.
/// This behavior uses IResultRequest{T} to obtain the inner type T from the request interface
/// rather than using reflection on the Result{T} response type.
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
    // Check if TRequest implements IResultRequest<T> to get inner type from the request interface
    // This is the key improvement: we get the type from the REQUEST interface (which we control)
    // rather than from the RESPONSE type Result<T> (from the Ardalis library)
    var resultRequestInterface = typeof(TRequest).GetInterfaces()
      .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IResultRequest<>));

    if (resultRequestInterface != null)
    {
      // Extract the inner type T from IResultRequest<T>
      var innerType = resultRequestInterface.GetGenericArguments()[0];

      // Call the generic factory method with the inner type
      // Note: We still use reflection to invoke the generic method, but the key improvement
      // is that we obtained the type from the request interface, not from Result<T>
      var method = typeof(CustomArdalisResultFactory)
        .GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
        .First(m => m.Name == factoryMethodName && m.IsGenericMethodDefinition);

      var genericMethod = method.MakeGenericMethod(innerType);
      var result = genericMethod.Invoke(null, new object[] { exception });
      return (TResponse)result!;
    }

    // If not a IResultRequest<T> type, rethrow the exception
    throw exception;
  }
}
