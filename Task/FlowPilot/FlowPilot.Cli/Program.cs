using System.Reflection;
using FlowPilot.Cli.Commands;
using FlowPilot.Cli.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace FlowPilot.Cli;

public class Program
{
  public static async Task<int> Main(string[] args)
  {
    // Check for --verbose flag
    var isVerbose = args.Contains("--verbose");

    // Remove --verbose from args if present so it doesn't interfere with command parsing
    var filteredArgs = args.Where(arg => arg != "--verbose").ToArray();

    // Configure Serilog with both console and file sinks
    var logFilePath = Path.Combine(Environment.GetEnvironmentVariable("HOME") ?? "/tmp", "flowpilot.log");
    var logLevel = isVerbose ? Serilog.Events.LogEventLevel.Debug : Serilog.Events.LogEventLevel.Information;

    Log.Logger = new LoggerConfiguration()
      .MinimumLevel.Is(logLevel)
      .WriteTo.Console()
      .WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 3)
      .CreateLogger();

    // Log version info with commit SHA before any other logging
    var assembly = Assembly.GetExecutingAssembly();
    var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unknown";
    var assemblyVersion = assembly.GetName().Version?.ToString() ?? "unknown";

    Log.Information("FlowPilot {AssemblyVersion}, commit {CommitSha}", assemblyVersion, version);
    Log.Information("FlowPilot starting - logs will be written to {LogFilePath}", logFilePath);

    try
    {
      // Build service provider
      var fileSystem = new FileSystemService();
      var currentDir = fileSystem.GetCurrentDirectory();

      var services = new ServiceCollection();
      services.AddLogging(builder => builder.AddSerilog(dispose: true));
      services.ConfigureServices(currentDir);
      var serviceProvider = services.BuildServiceProvider();

      // Default to help command if no arguments provided
      var commandName = filteredArgs.Length == 0 ? "help" : filteredArgs[0];
      var commandArgs = filteredArgs.Skip(1).ToArray();

      // Resolve command from keyed service
      var command = serviceProvider.GetKeyedService<ICommand>(commandName);

      if (command == null)
      {
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError("Unknown command '{CommandName}'", commandName);
        logger.LogInformation("Run 'flowpilot help' to see available commands.");
        return 1;
      }

      return await command.ExecuteAsync(commandArgs);
    }
    finally
    {
      await Log.CloseAndFlushAsync();
    }
  }
}
