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
    // Configure Serilog
    Log.Logger = new LoggerConfiguration()
      .MinimumLevel.Information()
      .WriteTo.Console()
      .CreateLogger();

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
      var commandName = args.Length == 0 ? "help" : args[0];
      var commandArgs = args.Skip(1).ToArray();

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
