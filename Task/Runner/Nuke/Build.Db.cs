using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Docker;
using Serilog;

public partial class Build
{
  [Parameter("Force operation without confirmation")]
  readonly bool Force;

  Target DbReset => _ => _
    .Description("Reset local SQL Server database by removing docker volume (confirm or --force to skip)")
    .Executes(() =>
    {
      if (!Force)
      {
        Console.Write("Are you sure? This will delete local dev database by removing the docker volume. [y/N] ");
        var response = Console.ReadLine();
        if (response?.ToLowerInvariant() != "y")
        {
          Console.WriteLine("Operation cancelled");
          return;
        }
      }

      ResetDatabase();
    });

  Target DbResetForce => _ => _
    .Description("Reset local dev database without confirmation by removing the docker volume")
    .Executes(ResetDatabase);

  void ResetDatabase()
  {
    var composeFile = TaskLocalDevDirectory / "docker-compose.yml";

    if (DoesDockerVolumeExist("localdev_sqlserver-data"))
    {
      Log.Information("Detected SQL Server docker volume. Removing volume to reset database...");
      RemoveSqlServerVolume(composeFile);
    }
    else
    {
      Log.Information("SQL Server docker volume not found. Nothing to do.");
    }
  }

  bool DoesDockerVolumeExist(string volumeName)
  {
    try
    {
      DockerTasks.DockerVolumeInspect(_ => _
        .SetVolumes(volumeName));
      return true;
    }
    catch
    {
      return false;
    }
  }

  void RemoveSqlServerVolume(string composeFile)
  {
    try
    {
      // Stop any running containers first
      Log.Information("Stopping SQL Server container if running...");
      DockerTasks.Docker($"compose -f {composeFile} down", workingDirectory: RootDirectory);

      // Remove the volume
      Log.Information("Removing SQL Server docker volume...");
      DockerTasks.DockerVolumeRm(_ => _
        .SetVolumes("localdev_sqlserver-data"));

      Log.Information("✓ SQL Server database reset complete - docker volume removed");
    }
    catch (Exception ex)
    {
      Log.Error("Failed to reset SQL Server database: {Message}", ex.Message);
      throw;
    }
  }

  Target DbMigrationsTestApply => _ => _
    .Description("Test EF Core migrations by applying them to a throwaway SQL Server database in Docker")
    .Executes(() =>
    {
      Log.Information("Testing migrations against a throwaway SQL Server database using Docker Compose");

      var composeFile = RootDirectory / "Test" / "Migrations" / "docker-compose.yml";

      int exitCode = 0;
      try
      {
        // Run docker-compose to start SQL Server and apply migrations
        Log.Information("Running Docker Compose to test migrations...");
        var args = $"compose -f {composeFile} up --build --abort-on-container-exit";
        var process = ProcessTasks.StartProcess("docker", args,
              workingDirectory: RootDirectory);
        process.WaitForExit();
        exitCode = process.ExitCode;
      }
      finally
      {
        // Clean up containers
        Log.Information("Cleaning up Docker Compose resources...");
        var downArgs = $"compose -f {composeFile} down";
        var downProcess = ProcessTasks.StartProcess("docker", downArgs,
              workingDirectory: RootDirectory,
              logOutput: false,
              logInvocation: false);
        downProcess.WaitForExit();
        Log.Information("✓ Docker Compose resources cleaned up");
      }

      // Explicitly fail the target if Docker Compose failed
      if (exitCode != 0)
      {
        Log.Error("Docker Compose exited with code: {ExitCode}", exitCode);
        throw new Exception($"Migration test failed with exit code: {exitCode}");
      }

      Log.Information("✓ Migrations applied successfully to test database");
    });
}
