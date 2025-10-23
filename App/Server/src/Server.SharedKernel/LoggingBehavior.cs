using System.Diagnostics;
using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Server.SharedKernel;

/// <summary>
/// Adds logging for all requests in MediatR pipeline.
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
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
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
      // Use structured logging to automatically capture request properties without reflection
      _logger.LogInformation("Handling {RequestName} with {@Request}", typeof(TRequest).Name, request);
    }

    var sw = Stopwatch.StartNew();

    var response = await next();

    _logger.LogInformation("Handled {RequestName} with {Response} in {ms} ms", typeof(TRequest).Name, response, sw.ElapsedMilliseconds);
    sw.Stop();
    return response;
  }
}
