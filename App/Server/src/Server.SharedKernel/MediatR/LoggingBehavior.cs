using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using Server.SharedKernel.Result;

namespace Server.SharedKernel.MediatR;

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
    if (_logger.IsEnabled(LogLevel.Debug))
    {
      _logger.LogDebug("Handling {RequestName}: {@Request}", typeof(TRequest).Name, request);
    }
    else
    {
      _logger.LogInformation("Handling {RequestName}", typeof(TRequest).Name);
    }

    var sw = Stopwatch.StartNew();

    var response = await next(cancellationToken);

    sw.Stop();

    var logLevel = GetLogLevel(response);

    if (logLevel == LogLevel.Information)
    {
      _logger.LogInformation(
        "Handled {RequestName} in {ms} ms status: {ResultStatus}",
        typeof(TRequest).Name,
        sw.ElapsedMilliseconds,
        response);
    }
    else
    {
      _logger.Log(
        logLevel,
        "Handled {RequestName} in {ms} ms with status: {ResultStatus} and result: {@Result}",
        typeof(TRequest).Name,
        sw.ElapsedMilliseconds,
        response.Status,
        response);
    }

    return response;
  }

  private LogLevel GetLogLevel(Result<T> result)
  {
    if (result.IsSuccess)
    {
      return _logger.IsEnabled(LogLevel.Debug) ? LogLevel.Debug : LogLevel.Information;
    }

    return result.Status == ResultStatus.Invalid ? LogLevel.Warning : LogLevel.Error;
  }
}
