using Nuke.Common;
using Nuke.Common.Tooling;
using Serilog;

public partial class Build
{
  [Parameter("Force operation without confirmation")]
  readonly bool Force = false;

  Target DbReset => _ => _
    .Description("Reset local database - removes SQL Server docker volume or deletes SQLite file (confirm or --force to skip)")
    .Executes(() =>
    {
      if (!Force)
      {
        Console.Write("Are you sure? This will delete all database data and schema. [y/N] ");
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
    .Description("Reset local database without confirmation - removes SQL Server docker volume or deletes SQLite file")
    .Executes(() =>
    {
      ResetDatabase();
    });

  void ResetDatabase()
  {
    var composeFile = RootDirectory / "Task" / "LocalDev" / "docker-compose.yml";

    // Check if docker-compose file exists and if SQL Server volume exists
    if (File.Exists(composeFile) && DoesDockerVolumeExist("localdev_sqlserver-data"))
    {
      Log.Information("Detected SQL Server docker volume. Removing volume to reset database...");
      RemoveSqlServerVolume(composeFile);
    }
    else
    {
      // Fallback to SQLite reset
      Log.Information("SQL Server volume not detected. Falling back to SQLite database reset...");
      Console.WriteLine($"Deleting {DatabaseFile}...");
      if (File.Exists(DatabaseFile))
      {
        File.Delete(DatabaseFile);
      }
      Console.WriteLine("Done.");
    }
  }

  bool DoesDockerVolumeExist(string volumeName)
  {
    try
    {
      var inspectArgs = $"volume inspect {volumeName}";
      var inspectProcess = ProcessTasks.StartProcess("docker", inspectArgs,
            workingDirectory: RootDirectory,
            logOutput: false,
            logInvocation: false);
      inspectProcess.WaitForExit();
      return inspectProcess.ExitCode == 0;
    }
    catch (Exception ex)
    {
      Log.Debug("Failed to check if docker volume exists: {Message}", ex.Message);
      return false;
    }
  }

  void RemoveSqlServerVolume(string composeFile)
  {
    try
    {
      // Stop any running containers first
      Log.Information("Stopping SQL Server container if running...");
      var downArgs = $"compose -f {composeFile} down";
      var downProcess = ProcessTasks.StartProcess("docker", downArgs,
            workingDirectory: RootDirectory);
      downProcess.WaitForExit();

      // Remove the volume
      Log.Information("Removing SQL Server docker volume...");
      var volumeRemoveArgs = "volume rm localdev_sqlserver-data";
      var volumeRemoveProcess = ProcessTasks.StartProcess("docker", volumeRemoveArgs,
            workingDirectory: RootDirectory);
      volumeRemoveProcess.WaitForExit();

      if (volumeRemoveProcess.ExitCode == 0)
      {
        Log.Information("✓ SQL Server database reset complete - docker volume removed");
      }
      else
      {
        Log.Error("Failed to remove docker volume. You may need to run: docker volume rm localdev_sqlserver-data");
        throw new Exception("Failed to remove SQL Server docker volume");
      }
    }
    catch (Exception ex)
    {
      Log.Error("Failed to reset SQL Server database: {Message}", ex.Message);
      throw;
    }
  }
}
