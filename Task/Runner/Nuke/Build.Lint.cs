using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Npm;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Npm.NpmTasks;

public partial class Build
{
  internal Target LintServerVerify => _ => _
      .Description("Verify backend formatting & analyzers (no changes). Fails if issues found")
      .Executes(() =>
      {
        Log.Information($"Running dotnet format (verify only) on {ServerSolution}");
        DotNetFormat(s => s
              .SetProject(ServerSolution)
              .SetVerifyNoChanges(true));
      });

  internal Target LintServerFix => _ => _
      .Description("Fix backend formatting & analyzer issues automatically")
      .Executes(() =>
      {
        Log.Information($"Running dotnet format (fix mode) on {ServerSolution}");
        DotNetFormat(s => s
              .SetProject(ServerSolution));
      });

  internal Target LintClientVerify => _ => _
      .Description("Verify client code formatting and style")
      .DependsOn(InstallClient)
      .Executes(() =>
      {
        Log.Information($"Running ESLint on {ClientDirectory}");
        NpmRun(s => s
              .SetProcessWorkingDirectory(ClientDirectory)
              .SetCommand("lint"));
      });

  internal Target LintClientFix => _ => _
      .Description("Fix client code formatting and style issues automatically")
      .DependsOn(InstallClient)
      .Executes(() =>
      {
        Log.Information($"Running ESLint fix on {ClientDirectory}");
        NpmRun(s => s
              .SetProcessWorkingDirectory(ClientDirectory)
              .SetCommand("lint:fix"));
      });

  internal Target LintNukeVerify => _ => _
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

  internal Target LintNukeFix => _ => _
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

  internal Target LintAllVerify => _ => _
      .Description("Verify all C# code formatting & analyzers (no changes). Fails if issues found")
      .DependsOn(LintClientVerify, LintServerVerify, LintNukeVerify)
      .Executes(() =>
      {
        var e2eTestProject = RootDirectory / "Test" / "e2e" / "E2eTests" / "E2eTests.csproj";

        Log.Information($"Running dotnet format (verify only) on {e2eTestProject}");
        DotNetFormat(s => s
              .SetProject(e2eTestProject)
              .SetVerifyNoChanges(true));
      });

  internal Target LintAllFix => _ => _
      .Description("Fix all C# formatting & analyzer issues automatically")
      .DependsOn(LintClientFix, LintServerFix, LintNukeFix)
      .Executes(() =>
      {
        var e2eTestProject = RootDirectory / "Test" / "e2e" / "E2eTests" / "E2eTests.csproj";

        Log.Information($"Running dotnet format (fix mode) on {e2eTestProject}");
        DotNetFormat(s => s
              .SetProject(e2eTestProject));
      });
}
