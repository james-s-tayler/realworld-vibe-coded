using System.Reflection;
using System.Text.RegularExpressions;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.IO;
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
      .DependsOn(LintClientVerify, LintServerVerify, LintNukeVerify, LintSkillsVerify, LintClaudeMdVerify)
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
      .DependsOn(LintClientFix, LintServerFix, LintNukeFix, LintSkillsFix)
      .Executes(() =>
      {
        var e2eTestProject = RootDirectory / "Test" / "e2e" / "E2eTests" / "E2eTests.csproj";

        Log.Information($"Running dotnet format (fix mode) on {e2eTestProject}");
        DotNetFormat(s => s
              .SetProject(e2eTestProject));
      });

  internal Target LintClaudeMdVerify => _ => _
      .Description("Verify CLAUDE.md stays within the 100-line limit for effective AI instruction following")
      .Executes(() =>
      {
        var lines = ClaudeMdFile.ReadAllLines();
        var lineCount = lines.Length;
        Log.Information("CLAUDE.md is {LineCount} lines (limit: 100)", lineCount);

        if (lineCount > 100)
        {
          throw new Exception($"CLAUDE.md is {lineCount} lines — exceeds the 100-line limit. Trim it to stay effective.");
        }

        Log.Information("✓ CLAUDE.md is within the 100-line limit");
      });

  internal Target LintSkillsVerify => _ => _
      .Description("Verify every Nuke target has a corresponding Claude Code skill")
      .Executes(() =>
      {
        var targets = GetAllExecutableTargets();
        var skillsDirectory = RootDirectory / ".claude" / "skills";
        var missing = new List<string>();

        foreach (var target in targets)
        {
          var kebabName = PascalToKebabCase(target.Name);
          var skillPath = skillsDirectory / $"nuke-{kebabName}" / "SKILL.md";

          if (!skillPath.FileExists())
          {
            missing.Add($"  {target.Name} → .claude/skills/nuke-{kebabName}/SKILL.md");
          }
        }

        if (missing.Any())
        {
          foreach (var entry in missing)
          {
            Log.Error("Missing skill: {Entry}", entry);
          }

          throw new Exception($"{missing.Count} Nuke target(s) missing Claude Code skills. Run './build.sh LintSkillsFix' to generate them.");
        }

        Log.Information("✓ All {Count} Nuke targets have corresponding Claude Code skills", targets.Count);
      });

  internal Target LintSkillsFix => _ => _
      .Description("Generate missing Claude Code skills for Nuke targets")
      .Executes(() =>
      {
        var targets = GetAllExecutableTargets();
        var skillsDirectory = RootDirectory / ".claude" / "skills";
        var created = 0;

        foreach (var target in targets)
        {
          var kebabName = PascalToKebabCase(target.Name);
          var skillDir = skillsDirectory / $"nuke-{kebabName}";
          var skillPath = skillDir / "SKILL.md";

          if (skillPath.FileExists())
          {
            continue;
          }

          skillDir.CreateDirectory();
          var content = GenerateSkillContent(target.Name, target.Description);
          skillPath.WriteAllText(content);
          created++;
          Log.Information("Created skill: nuke-{KebabName}/SKILL.md", kebabName);
        }

        if (created > 0)
        {
          Log.Information("✓ Created {Count} missing skill(s)", created);
        }
        else
        {
          Log.Information("✓ All skills already exist — nothing to do");
        }
      });

  private static string PascalToKebabCase(string name)
  {
    var result = Regex.Replace(name, "([a-z0-9])([A-Z])", "$1-$2");
    result = Regex.Replace(result, "([A-Z])([A-Z][a-z])", "$1-$2");
    return result.ToLowerInvariant();
  }

  private static string GenerateSkillContent(string targetName, string description)
  {
    return string.Join(
        "\n",
        "---",
        $"description: {description}",
        "---",
        string.Empty,
        $"Run `./build.sh {targetName}`. Check output for details. If failures occur, check `Reports/` and `Logs/` directories.",
        string.Empty);
  }

  private IReadOnlyCollection<ExecutableTarget> GetAllExecutableTargets()
  {
    return (IReadOnlyCollection<ExecutableTarget>)typeof(NukeBuild)
        .GetProperty("ExecutableTargets", BindingFlags.Instance | BindingFlags.NonPublic)!
        .GetValue(this)!;
  }
}
