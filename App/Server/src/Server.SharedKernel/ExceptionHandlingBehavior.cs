using MediatR;
using Microsoft.Extensions.Logging;
using Server.SharedKernel.Result;

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
    catch (Exception ex)
    {
      _logger.LogError(ex, "An unhandled exception occurred while processing {RequestName}", typeof(TRequest).Name);

      // Create validation errors from the exception
      var validationErrors = new[]
      {
        new ValidationError("exception.type", ex.GetType().Name),
        new ValidationError("exception.message", ex.Message)
      };

      // Create a CriticalError result with validation errors
      var resultType = typeof(TResponse);
      if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Result<>))
      {
        var valueType = resultType.GetGenericArguments()[0];
        var criticalErrorMethod = resultType.GetMethod(nameof(Result<object>.CriticalError), new[] { typeof(ValidationError[]) });

        if (criticalErrorMethod != null)
        {
          var result = criticalErrorMethod.Invoke(null, new object[] { validationErrors });
          return (TResponse)result!;
        }
      }

      // Fallback for non-generic Result types
      throw new InvalidOperationException($"Unable to create CriticalError result for type {resultType.Name}", ex);
    }
  }
}
