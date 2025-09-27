using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;
using FastEndpoints;
using Microsoft.Extensions.Logging;

namespace Server.Web.Infrastructure;

public sealed class OpenTelemetryCommandLogger<TCommand, TResult>(
    ILogger<TCommand> logger,
    ActivitySource activitySource,
    Meter meter)
    : ICommandMiddleware<TCommand, TResult> where TCommand : FastEndpoints.ICommand<TResult>
{
  private readonly ILogger<TCommand> _logger = logger;
  private readonly ActivitySource _activitySource = activitySource;
  private readonly Counter<long> _commandCounter = meter.CreateCounter<long>(
    "conduit.commands.total", 
    "commands", 
    "Total number of commands executed");
  private readonly Histogram<double> _commandDuration = meter.CreateHistogram<double>(
    "conduit.commands.duration", 
    "milliseconds", 
    "Duration of command execution");

  public async Task<TResult> ExecuteAsync(TCommand command,
                                          CommandDelegate<TResult> next,
                                          CancellationToken ct)
  {
    string commandName = command.GetType().Name;
    
    using var activity = _activitySource.StartActivity($"Command.{commandName}");
    activity?.SetTag("command.name", commandName);
    activity?.SetTag("command.type", typeof(TCommand).FullName);
    
    if (_logger.IsEnabled(LogLevel.Information))
    {
      _logger.LogInformation("Handling {RequestName}", commandName);

      // Reflection! Could be a performance concern
      Type myType = command.GetType();
      IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties());
      foreach (PropertyInfo prop in props)
      {
        object? propValue = prop?.GetValue(command, null);
        _logger.LogInformation("Property {Property} : {@Value}", prop?.Name, propValue);
        
        // Add property to activity for tracing
        if (propValue != null && activity != null)
        {
          activity.SetTag($"command.property.{prop?.Name?.ToLowerInvariant()}", propValue.ToString());
        }
      }
    }

    var sw = Stopwatch.StartNew();

    try
    {
      var result = await next();
      
      sw.Stop();
      
      // Record metrics
      _commandCounter.Add(1, new KeyValuePair<string, object?>("command.name", commandName), 
                              new KeyValuePair<string, object?>("status", "success"));
      _commandDuration.Record(sw.ElapsedMilliseconds, 
                              new KeyValuePair<string, object?>("command.name", commandName),
                              new KeyValuePair<string, object?>("status", "success"));
      
      activity?.SetTag("command.status", "success");
      activity?.SetTag("command.duration_ms", sw.ElapsedMilliseconds);
      
      _logger.LogInformation("Handled {CommandName} with {Result} in {ms} ms", commandName, result, sw.ElapsedMilliseconds);
      
      return result;
    }
    catch (Exception ex)
    {
      sw.Stop();
      
      // Record failure metrics
      _commandCounter.Add(1, new KeyValuePair<string, object?>("command.name", commandName), 
                              new KeyValuePair<string, object?>("status", "error"));
      _commandDuration.Record(sw.ElapsedMilliseconds, 
                              new KeyValuePair<string, object?>("command.name", commandName),
                              new KeyValuePair<string, object?>("status", "error"));
      
      activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
      activity?.SetTag("command.status", "error");
      activity?.SetTag("command.error", ex.Message);
      activity?.SetTag("command.duration_ms", sw.ElapsedMilliseconds);
      
      _logger.LogError(ex, "Command {CommandName} failed after {ms} ms", commandName, sw.ElapsedMilliseconds);
      
      throw;
    }
  }
}