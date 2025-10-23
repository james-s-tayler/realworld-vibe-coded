using System.Diagnostics;
using Ardalis.GuardClauses;
using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Server.SharedKernel;

/// <summary>
/// Adds logging for all requests in MediatR pipeline.
/// Works with any IRequest but provides enhanced logging for Result&lt;T&gt; responses.
/// Configure by adding the service with a scoped lifetime
/// 
/// Example for Autofac:
/// builder
///   .RegisterType&lt;Mediator&gt;()
///   .As&lt;IMediator&gt;()
///   .InstancePerLifetimeScope();
///
/// builder
///   .RegisterGeneric(typeof(LoggingBehavior&lt;,&gt;))
///      .As(typeof(IPipelineBehavior&lt;,&gt;))
///   .InstancePerLifetimeScope();
///
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
  where TRequest : IRequest<TResponse>
{
  private readonly ILogger<Mediator> _logger;

  public LoggingBehavior(ILogger<Mediator> logger)
  {
    _logger = logger;
  }

  public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
  {
    Guard.Against.Null(request);

    if (_logger.IsEnabled(LogLevel.Information))
    {
      _logger.LogInformation("Handling {RequestName}: {@Request}", typeof(TRequest).Name, request);
    }

    var sw = Stopwatch.StartNew();

    var response = await next();

    // Check if response is a Result<T> by checking the type
    var responseType = response?.GetType();
    if (responseType != null && responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
    {
      // Use dynamic to get Status property value, then cast it for logging
      dynamic resultDynamic = response!;
      ResultStatus status = resultDynamic.Status;
      _logger.LogInformation("Handled {RequestName} with {Status} in {ms} ms",
        typeof(TRequest).Name, status, sw.ElapsedMilliseconds);
    }
    else
    {
      _logger.LogInformation("Handled {RequestName} in {ms} ms",
        typeof(TRequest).Name, sw.ElapsedMilliseconds);
    }

    sw.Stop();

    return response;
  }
}
