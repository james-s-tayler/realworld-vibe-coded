using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Npm;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Npm.NpmTasks;

public partial class Build
{
  Target LintServerVerify => _ => _
      .Description("Verify backend formatting & analyzers (no changes). Fails if issues found")
      .Executes(() =>
      {
        Log.Information($"Running dotnet format (verify only) on {ServerSolution}");
        DotNetFormat(s => s
              .SetProject(ServerSolution)
              .SetVerifyNoChanges(true));
      });

  Target LintServerFix => _ => _
      .Description("Fix backend formatting & analyzer issues automatically")
      .Executes(() =>
      {
        Log.Information($"Running dotnet format (fix mode) on {ServerSolution}");
        DotNetFormat(s => s
              .SetProject(ServerSolution));
      });

  Target LintClientVerify => _ => _
      .Description("Verify client code formatting and style")
      .DependsOn(InstallClient)
      .Executes(() =>
      {
        Log.Information($"Running ESLint on {ClientDirectory}");
        NpmRun(s => s
              .SetProcessWorkingDirectory(ClientDirectory)
              .SetCommand("lint"));
      });

  Target LintClientFix => _ => _
      .Description("Fix client code formatting and style issues automatically")
      .DependsOn(InstallClient)
      .Executes(() =>
      {
        Log.Information($"Running ESLint fix on {ClientDirectory}");
        NpmRun(s => s
              .SetProcessWorkingDirectory(ClientDirectory)
              .SetCommand("lint:fix"));
      });

  Target LintNukeVerify => _ => _
      .Description("Verify Nuke build targets for documentation and naming conventions")
      .Executes(() =>
      {
        var nukeSolution = TaskRunnerDirectory / "Nuke.sln";

        // Run dotnet format on the Nuke solution (only check whitespace and style, not warnings)
        Log.Information($"Running dotnet format (verify only) on {nukeSolution}");
        DotNetFormat(s => s
              .SetProject(nukeSolution)
              .SetSeverity("error")
              .SetVerifyNoChanges(true));

        // Run the ArchUnit tests
        var testProject = TaskRunnerDirectory / "Nuke.Tests" / "Nuke.Tests.csproj";
        DotNetTest(s => s
              .SetProjectFile(testProject));
      });

  Target LintNukeFix => _ => _
      .Description("Fix Nuke build formatting and style issues automatically")
      .Executes(() =>
      {
        var nukeSolution = TaskRunnerDirectory / "Nuke.sln";

        // Run dotnet format on the Nuke solution to fix formatting issues
        Log.Information($"Running dotnet format (fix mode) on {nukeSolution}");
        DotNetFormat(s => s
              .SetProject(nukeSolution)
              .SetSeverity("error"));
      });
}
