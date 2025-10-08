using System.Text;
using Nuke.Common;
using Nuke.Common.Tooling;
using Serilog;

public partial class Build
{
  [Parameter("Base branch to compare migrations against (default: main)")]
  readonly string BaseBranch = "main";

  Target DbMigrationsCheckDataLoss => _ => _
    .Description("Check EF Core migrations for potentially destructive operations that could cause data loss")
    .Executes(() =>
    {
      Log.Information("Checking migrations for destructive operations in {MigrationsDirectory}", MigrationsDirectory);
      Log.Information("Comparing against base branch: {BaseBranch}", BaseBranch);

      // Get list of changed migration files using git diff
      var gitDiffArgs = $"diff --name-only {BaseBranch}...HEAD -- {MigrationsDirectory}";
      var gitProcess = ProcessTasks.StartProcess("git", gitDiffArgs,
        workingDirectory: RootDirectory,
        logOutput: false);
      gitProcess.AssertZeroExitCode();

      var changedFiles = gitProcess.Output
        .Where(o => o.Type == OutputType.Std)
        .Select(o => o.Text.Trim())
        .Where(f => f.EndsWith(".cs") && !f.EndsWith(".Designer.cs") && !f.EndsWith("ModelSnapshot.cs"))
        .Select(f => RootDirectory / f)
        .Where(f => File.Exists(f))
        .ToList();

      // Define output file path
      var outputFilePath = RootDirectory / "migration-check-output.md";

      if (!changedFiles.Any())
      {
        Log.Information("✓ No new or modified migrations found in this PR");

        // Write empty markdown file to indicate no issues
        File.WriteAllText(outputFilePath, "✅ No new or modified migrations in this PR.");
        Log.Information("Output written to {OutputFile}", outputFilePath);
        return;
      }

      Log.Information("Found {Count} new/modified migration file(s) in this PR", changedFiles.Count);

      var destructiveOperations = new[]
      {
        "DropTable",
        "DropColumn",
        "DropIndex",
        "DropForeignKey",
        "DropPrimaryKey",
        "AlterColumn"  // Can cause data loss if column type changes
      };

      var migrationsWithDestructiveOps = new List<(string File, List<string> Operations)>();

      foreach (var migrationFile in changedFiles)
      {
        var content = File.ReadAllText(migrationFile);
        var foundOps = destructiveOperations
          .Where(op => content.Contains($"migrationBuilder.{op}"))
          .ToList();

        if (foundOps.Any())
        {
          migrationsWithDestructiveOps.Add((Path.GetFileName(migrationFile), foundOps));
        }
      }

      // Generate markdown output
      var markdown = new StringBuilder();

      if (migrationsWithDestructiveOps.Any())
      {
        markdown.AppendLine("## ⚠️ Migration Data Loss Warning");
        markdown.AppendLine();
        markdown.AppendLine($"Found **{migrationsWithDestructiveOps.Count}** migration(s) with potentially destructive operations:");
        markdown.AppendLine();

        foreach (var (file, ops) in migrationsWithDestructiveOps)
        {
          markdown.AppendLine($"### 📄 `{file}`");
          markdown.AppendLine();
          markdown.AppendLine("Destructive operations detected:");
          foreach (var op in ops)
          {
            markdown.AppendLine($"- `{op}`");
          }
          markdown.AppendLine();
        }

        markdown.AppendLine("---");
        markdown.AppendLine();
        markdown.AppendLine("**⚠️ IMPORTANT:** These migrations contain operations that may cause data loss.");
        markdown.AppendLine();
        markdown.AppendLine("Please ensure:");
        markdown.AppendLine("- Manual data migration steps are included if needed");
        markdown.AppendLine("- Custom SQL or code preserves existing data where appropriate");
        markdown.AppendLine("- These changes have been reviewed and approved");

        Log.Warning("Found {Count} migration(s) with potentially destructive operations", migrationsWithDestructiveOps.Count);
        foreach (var (file, ops) in migrationsWithDestructiveOps)
        {
          Log.Warning("  {File}: {Operations}", file, string.Join(", ", ops));
        }
      }
      else
      {
        markdown.AppendLine("✅ No destructive operations found in new/modified migrations.");
        Log.Information("✓ No destructive operations found in new/modified migrations");
      }

      // Write markdown output to file
      File.WriteAllText(outputFilePath, markdown.ToString());
      Log.Information("Migration check output written to {OutputFile}", outputFilePath);
    });

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
