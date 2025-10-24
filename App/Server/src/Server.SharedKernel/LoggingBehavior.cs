using System.Diagnostics;
using System.Reflection;
using Ardalis.GuardClauses;
using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Server.SharedKernel;

/// <summary>
/// MediatR pipeline behavior that adds logging for all requests in the pipeline.
/// Logs request handling start, request properties, and completion with timing.
/// This behavior uses IResultRequest{T} to align with the constrained generic pattern.
/// </summary>
/// <typeparam name="TRequest">The request type implementing IResultRequest{T}</typeparam>
/// <typeparam name="T">The inner value type of Result{T}</typeparam>
public class LoggingBehavior<TRequest, T> : IPipelineBehavior<TRequest, Result<T>>
  where TRequest : IResultRequest<T>
{
  private readonly ILogger<LoggingBehavior<TRequest, T>> _logger;

  public LoggingBehavior(ILogger<LoggingBehavior<TRequest, T>> logger)
  {
    _logger = logger;
  }

  public async Task<Result<T>> Handle(TRequest request, RequestHandlerDelegate<Result<T>> next, CancellationToken cancellationToken)
  {
    Guard.Against.Null(request);
    if (_logger.IsEnabled(LogLevel.Information))
    {
      _logger.LogInformation("Handling {RequestName}", typeof(TRequest).Name);

      // Reflection! Could be a performance concern
      Type myType = request.GetType();
      IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties());
      foreach (PropertyInfo prop in props)
      {
        object? propValue = prop?.GetValue(request, null);
        _logger.LogInformation("Property {Property} : {@Value}", prop?.Name, propValue);
      }
    }

    var sw = Stopwatch.StartNew();

    var response = await next();

    _logger.LogInformation("Handled {RequestName} with {Response} in {ms} ms", typeof(TRequest).Name, response, sw.ElapsedMilliseconds);
    sw.Stop();
    return response;
  }
}
