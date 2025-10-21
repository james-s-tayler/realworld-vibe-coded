using Ardalis.Result;

namespace Server.Web.Infrastructure;

/// <summary>
/// MediatR pipeline behavior that catches exceptions thrown in handlers
/// and converts them to Result.Error for consistent error handling.
/// Note: This behavior applies to all MediatR handlers but only converts exceptions to Result.Error
/// when the handler returns a Result&lt;T&gt; type. Other return types will re-throw the exception.
/// </summary>
public class ExceptionHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
  where TRequest : notnull
  where TResponse : class
{
  private readonly ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> _logger;

  public ExceptionHandlingBehavior(ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> logger)
  {
    _logger = logger;
  }

  public async Task<TResponse> Handle(
    TRequest request,
    RequestHandlerDelegate<TResponse> next,
    CancellationToken cancellationToken)
  {
    try
    {
      return await next();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Exception occurred while handling {RequestType}: {Message}",
        typeof(TRequest).Name, ex.Message);

      // Check if TResponse is a Result type
      var responseType = typeof(TResponse);
      if (responseType.IsGenericType)
      {
        var genericTypeDefinition = responseType.GetGenericTypeDefinition();
        if (genericTypeDefinition == typeof(Result<>))
        {
          // Create Result<T>.Error(message)
          var resultType = responseType;
          var errorMethod = resultType.GetMethod("Error", new[] { typeof(string) });
          if (errorMethod != null)
          {
            var result = errorMethod.Invoke(null, new object[] { ex.Message });
            return (TResponse)result!;
          }
        }
      }

      // If we can't convert to Result, re-throw and let the global exception handler deal with it
      throw;
    }
  }
}
