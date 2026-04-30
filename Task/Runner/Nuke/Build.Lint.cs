using System.Text.Json;
using Json.Schema;
using Nuke.Common;
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

  internal Target LintAppSettingsVerify => _ => _
      .Description("Validate feature_management sections in appsettings*.json against the vendored v2 JSON schema")
      .Executes(() =>
      {
        var schemaPath = SchemaDirectory / "FeatureManagement.v2.0.0.schema.json";
        var schemaText = schemaPath.ReadAllText();
        var buildOptions = new BuildOptions { Dialect = Dialect.Draft07 };
        var schema = JsonSchema.FromText(schemaText, buildOptions);

        var appSettingsFiles = AppSettingsDirectory.GlobFiles("appsettings*.json");
        var errors = new List<string>();

        foreach (var file in appSettingsFiles)
        {
          var content = file.ReadAllText();
          var jsonOptions = new JsonDocumentOptions
          {
            CommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
          };
          var doc = JsonDocument.Parse(content, jsonOptions);

          if (!doc.RootElement.TryGetProperty("feature_management", out var featureSection))
          {
            Log.Warning("{File} has no feature_management section — skipping", file.Name);
            continue;
          }

          // Wrap the section to match the schema's root structure
          var wrapped = JsonDocument.Parse(JsonSerializer.Serialize(new { feature_management = featureSection }));

          var options = new EvaluationOptions
          {
            OutputFormat = OutputFormat.List,
          };

          var result = schema.Evaluate(wrapped.RootElement, options);

          if (!result.IsValid)
          {
            errors.Add($"  {file.Name}:");

            if (result.Details != null)
            {
              foreach (var detail in result.Details.Where(d => d.Errors != null))
              {
                foreach (var error in detail.Errors!)
                {
                  errors.Add($"    [{detail.InstanceLocation}] {error.Key}: {error.Value}");
                }
              }
            }
          }
          else
          {
            Log.Information("✓ {File} feature_management section is valid", file.Name);
          }
        }

        if (errors.Any())
        {
          foreach (var error in errors)
          {
            Log.Error("{Error}", error);
          }

          throw new Exception("feature_management schema validation failed.");
        }

        Log.Information("✓ All appsettings feature_management sections pass schema validation");
      });

  internal Target LintAllVerify => _ => _
      .Description("Verify all C# code formatting & analyzers (no changes). Fails if issues found")
      .DependsOn(LintClientVerify, LintServerVerify, LintNukeVerify, LintClaudeMdVerify, LintClaudeRulesVerify, LintApiClientVerify, LintAppSettingsVerify, LintSpecVerify)
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

  internal Target LintClaudeMdVerify => _ => _
      .Description("Verify CLAUDE.md stays within the 50-line limit and Rules Index table covers all rules files")
      .Executes(() =>
      {
        var lines = ClaudeMdFile.ReadAllLines();
        var lineCount = lines.Length;
        Log.Information("CLAUDE.md is {LineCount} lines (limit: 50)", lineCount);

        if (lineCount > 50)
        {
          throw new Exception($"CLAUDE.md is {lineCount} lines — exceeds the 50-line limit. Trim it to stay effective.");
        }

        Log.Information("✓ CLAUDE.md is within the 50-line limit");

        var rulesFiles = ClaudeRulesDirectory.GlobFiles("*.md")
            .Select(f => f.Name)
            .OrderBy(f => f)
            .ToList();

        var tableEntries = lines
            .Where(l => l.TrimStart().StartsWith("| `") && l.Contains(".md`"))
            .Select(l => l.Split('`')[1])
            .OrderBy(f => f)
            .ToList();

        var missing = rulesFiles.Except(tableEntries).ToList();
        var stale = tableEntries.Except(rulesFiles).ToList();

        if (missing.Any() || stale.Any())
        {
          if (missing.Any())
          {
            Log.Error("Rules files missing from CLAUDE.md Rules Index table: {Files}", string.Join(", ", missing));
          }

          if (stale.Any())
          {
            Log.Error("Stale entries in CLAUDE.md Rules Index table (file no longer exists): {Files}", string.Join(", ", stale));
          }

          throw new Exception("CLAUDE.md Rules Index table is out of sync with .claude/rules/*.md files.");
        }

        Log.Information("✓ Rules Index table covers all {Count} rules files", rulesFiles.Count);
      });

  internal Target LintClaudeRulesVerify => _ => _
      .Description("Verify all .claude/rules/*.md files are within the 85-line limit")
      .Executes(() =>
      {
        const int maxLines = 85;
        var files = ClaudeRulesDirectory.GlobFiles("*.md");
        var violations = new List<string>();

        foreach (var file in files)
        {
          var lineCount = file.ReadAllLines().Length;

          if (lineCount > maxLines)
          {
            violations.Add($"  {file.Name}: {lineCount} lines (limit: {maxLines})");
          }
        }

        if (violations.Any())
        {
          foreach (var violation in violations)
          {
            Log.Error("Over limit: {Violation}", violation);
          }

          throw new Exception($"{violations.Count} rules file(s) exceed the {maxLines}-line limit. Split them into smaller files.");
        }

        Log.Information("✓ All {Count} rules files are within the {MaxLines}-line limit", files.Count, maxLines);
      });

  internal Target LintSpecVerify => _ => _
      .Description("Verify SPEC-REFERENCE.md has all required sections for complete eval coverage")
      .Executes(() =>
      {
        var specFile = RootDirectory / "SPEC-REFERENCE.md";
        if (!specFile.FileExists())
        {
          throw new Exception($"Spec file not found: {specFile}");
        }

        var script = RootDirectory / "scripts" / "evals" / "lint-spec.sh";
        if (!script.FileExists())
        {
          throw new Exception($"Lint script not found: {script}");
        }

        Log.Information("Linting spec: {File}", specFile);

        var process = ProcessTasks.StartProcess(
          "bash",
          $"{script} --spec {specFile}",
          workingDirectory: RootDirectory,
          logOutput: true);

        process.AssertZeroExitCode();

        Log.Information("✓ Spec passes structural completeness checks");
      });
}
