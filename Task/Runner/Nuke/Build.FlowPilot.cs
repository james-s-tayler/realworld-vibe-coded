using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

public partial class Build
{
  internal Target BuildFlowPilot => _ => _
    .Description("Build FlowPilot CLI")
    .Executes(() =>
    {
      Log.Information($"Building FlowPilot solution at {FlowPilotSolution}");
      DotNetBuild(s => s.SetProjectFile(FlowPilotSolution));
    });

  internal Target TestFlowPilot => _ => _
    .Description("Run FlowPilot tests")
    .DependsOn(BuildFlowPilot)
    .DependsOn(RunLocalCleanDirectories)
    .Executes(() =>
    {
      Log.Information($"Running FlowPilot tests");

      var testProject = FlowPilotTestProject;
      var logFileName = "FlowPilot.Tests-results.trx";

      var failures = new List<string>();

      try
      {
        DotNetTest(s => s
          .SetProjectFile(testProject)
          .SetLoggers($"trx;LogFileName={logFileName}")
          .SetResultsDirectory(ReportsFlowPilotResultsDirectory));
      }
      catch (ProcessException)
      {
        failures.Add(testProject.Name);
      }

      // Run Docker-based integration tests
      Log.Information("Running Docker-based integration tests for FlowPilot CLI");
      var integrationTestDirectory = FlowPilotDirectory / "FlowPilot.Tests.Integration";

      try
      {
        ProcessTasks.StartProcess(
          "docker",
          "compose up --build --abort-on-container-exit --exit-code-from flowpilot-integration-test",
          workingDirectory: integrationTestDirectory)
          .AssertZeroExitCode();

        Log.Information("✅ Docker integration tests passed");
      }
      catch (ProcessException)
      {
        Log.Error("❌ Docker integration tests failed");
        failures.Add("FlowPilot.Tests.Integration (Docker)");
      }

      if (failures.Any())
      {
        var failedProjects = string.Join(", ", failures);
        throw new Exception($"FlowPilot tests failed: {failedProjects}");
      }
    });

  internal Target LintFlowPilotVerify => _ => _
    .Description("Verify FlowPilot code formatting & analyzers (no changes)")
    .Executes(() =>
    {
      Log.Information($"Running dotnet format (verify only) on {FlowPilotSolution}");
      DotNetFormat(s => s
        .SetProject(FlowPilotSolution)
        .SetVerifyNoChanges(true));
    });

  internal Target LintFlowPilotFix => _ => _
    .Description("Fix FlowPilot formatting & analyzer issues automatically")
    .Executes(() =>
    {
      Log.Information($"Running dotnet format (fix mode) on {FlowPilotSolution}");
      DotNetFormat(s => s
        .SetProject(FlowPilotSolution));
    });

  internal Target BuildFlowPilotPublish => _ => _
    .Description("Publish FlowPilot CLI as a NuGet package to local feed")
    .DependsOn(BuildFlowPilot)
    .Executes(() =>
    {
      // Ensure local NuGet feed directory exists
      LocalNuGetFeedDirectory.CreateOrCleanDirectory();
      Log.Information($"Local NuGet feed directory: {LocalNuGetFeedDirectory}");

      // Pack the FlowPilot.Cli project
      Log.Information($"Packing FlowPilot.Cli from {FlowPilotCliProject}");
      DotNetPack(s => s
        .SetProject(FlowPilotCliProject)
        .SetConfiguration("Release")
        .SetOutputDirectory(LocalNuGetFeedDirectory)
        .SetProperty("PackAsTool", "true"));

      Log.Information($"✅ FlowPilot.Cli published to {LocalNuGetFeedDirectory}");
    });

  internal Target InstallFlowPilot => _ => _
    .Description("Install FlowPilot CLI as a global dotnet tool from local feed")
    .DependsOn(BuildFlowPilotPublish)
    .Executes(() =>
    {
      try
      {
        Log.Information("Uninstalling existing FlowPilot.Cli tool if present...");
        DotNetToolUninstall(s => s
          .SetPackageName("FlowPilot.Cli")
          .SetGlobal(true));
      }
      catch
      {
        Log.Information("No existing FlowPilot.Cli tool found to uninstall.");
      }

      Log.Information($"Installing FlowPilot.Cli from local feed {LocalNuGetFeedDirectory}...");
      DotNetToolInstall(s => s
        .SetPackageName("FlowPilot.Cli")
        .SetGlobal(true)
        .AddSources(LocalNuGetFeedDirectory));

      Log.Information("✅ FlowPilot.Cli installed successfully. Run 'flowpilot' to verify.");
    });
}
