using Nuke.Common;
using Nuke.Common.Tooling;
using Serilog;

public partial class Build
{
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
