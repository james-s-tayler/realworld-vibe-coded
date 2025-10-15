using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Npm;
using Serilog;
using static Nuke.Common.Tools.Npm.NpmTasks;

public partial class Build
{
  Target RunLocalServer => _ => _
    .Description("Run backend locally using Docker Compose with SQL Server and hot-reload")
    .Executes(() =>
    {
      Log.Information("Starting local development environment with Docker Compose...");

      var composeFile = RootDirectory / "Task" / "LocalDev" / "docker-compose.yml";

      try
      {
        // Run docker-compose to start SQL Server and API with hot-reload
        Log.Information("Running Docker Compose for local development...");
        var args = $"compose -f {composeFile} up --build";
        var process = ProcessTasks.StartProcess("docker", args,
              workingDirectory: RootDirectory);
        process.WaitForExit();
      }
      finally
      {
        // Clean up containers when user stops the process
        Log.Information("Cleaning up Docker Compose resources...");
        var downArgs = $"compose -f {composeFile} down";
        var downProcess = ProcessTasks.StartProcess("docker", downArgs,
              workingDirectory: RootDirectory,
              logOutput: false,
              logInvocation: false);
        downProcess.WaitForExit();
        Log.Information("✓ Docker Compose resources cleaned up");
      }
    });

  Target RunLocalClient => _ => _
    .Description("Run client locally")
    .DependsOn(InstallClient)
    .Executes(() =>
    {
      Log.Information($"Starting Vite dev server in {ClientDirectory}");
      NpmRun(s => s
        .SetProcessWorkingDirectory(ClientDirectory)
        .SetCommand("dev"));
    });
}
