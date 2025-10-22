using MediatR;

namespace Server.Infrastructure.Data;

/// <summary>
/// MediatR pipeline behavior that catches DbUpdateConcurrencyException during request handling
/// and transforms them into Result.Conflict responses.
/// This behavior should be registered before ExceptionHandlingBehavior in the pipeline.
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class ConcurrencyExceptionHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
  where TRequest : IRequest<TResponse>
{
  private readonly ILogger<ConcurrencyExceptionHandlingBehavior<TRequest, TResponse>> _logger;

  public ConcurrencyExceptionHandlingBehavior(ILogger<ConcurrencyExceptionHandlingBehavior<TRequest, TResponse>> logger)
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
      _logger.LogWarning(ex, "A concurrency conflict occurred while processing {RequestName}", typeof(TRequest).Name);

      // Handle concurrency exceptions with Result.Conflict
      var resultType = typeof(TResponse);
      if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Ardalis.Result.Result<>))
      {
        var valueType = resultType.GetGenericArguments()[0];
        var resultFactoryType = typeof(Ardalis.Result.Result<>).MakeGenericType(valueType);
        var conflictMethod = resultFactoryType.GetMethod(nameof(Ardalis.Result.Result<object>.Conflict), new[] { typeof(string) });

        if (conflictMethod != null)
        {
          var result = conflictMethod.Invoke(null, new object[] { "The resource was modified by another user. Please refresh and try again." });
          return (TResponse)result!;
        }
      }

      // Fallback: re-throw if we can't create a Result.Conflict
      throw;
    }
  }
}
