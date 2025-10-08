using Nuke.Common;
using Nuke.Common.Tooling;
using Serilog;

public partial class Build
{
  Target DbMigrationsCheckUncommitted => _ => _
    .Description("Check for uncommitted EF Core migrations in source control")
    .Executes(() =>
    {
      Log.Information("Checking for uncommitted migrations in {MigrationsDirectory}", MigrationsDirectory);

      // Get list of untracked and modified files in git
      var gitStatusOutput = ProcessTasks.StartProcess("git",
          "status --porcelain",
          workingDirectory: RootDirectory)
        .AssertZeroExitCode()
        .Output;

      var uncommittedMigrationFiles = gitStatusOutput
        .Where(o => o.Text.Contains("Migrations/") &&
                    (o.Text.StartsWith("??") || o.Text.StartsWith(" M") || o.Text.StartsWith("A ")))
        .Select(o => o.Text)
        .ToList();

      if (uncommittedMigrationFiles.Any())
      {
        Log.Error("Found {Count} uncommitted migration file(s):", uncommittedMigrationFiles.Count);
        foreach (var file in uncommittedMigrationFiles)
        {
          Log.Error("  {File}", file);
        }
        throw new Exception("Uncommitted migrations detected. Please commit all migrations to source control before proceeding.");
      }

      Log.Information("✓ No uncommitted migrations found");
    });

  Target DbMigrationsCheckDataLoss => _ => _
    .Description("Check EF Core migrations for potentially destructive operations that could cause data loss")
    .Executes(() =>
    {
      Log.Information("Checking migrations for destructive operations in {MigrationsDirectory}", MigrationsDirectory);

      var migrationFiles = Directory.GetFiles(MigrationsDirectory, "*.cs")
        .Where(f => !f.EndsWith(".Designer.cs") && !f.EndsWith("ModelSnapshot.cs"))
        .ToList();

      if (!migrationFiles.Any())
      {
        Log.Warning("No migration files found in {MigrationsDirectory}", MigrationsDirectory);
        return;
      }

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

      foreach (var migrationFile in migrationFiles)
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

      if (migrationsWithDestructiveOps.Any())
      {
        Log.Warning("Found {Count} migration(s) with potentially destructive operations:", migrationsWithDestructiveOps.Count);
        foreach (var (file, ops) in migrationsWithDestructiveOps)
        {
          Log.Warning("  {File}:", file);
          foreach (var op in ops)
          {
            Log.Warning("    - {Operation}", op);
          }
        }

        Log.Warning("");
        Log.Warning("⚠ IMPORTANT: These migrations contain operations that may cause data loss.");
        Log.Warning("Please review these migrations and ensure they include manual data migration steps if needed.");
        Log.Warning("Consider adding custom SQL or data migration code to preserve existing data.");

        // Note: We're not throwing an exception here to allow manual review.
        // In a real-world scenario, you might want to add a --fail-on-destructive flag
        // or check for comments indicating manual review was done.
      }
      else
      {
        Log.Information("✓ No destructive operations found in migrations");
      }
    });

  Target DbMigrationsTestApply => _ => _
    .Description("Test EF Core migrations by applying them to a throwaway SQL Server database in Docker")
    .Executes(() =>
    {
      Log.Information("Testing migrations against a throwaway SQL Server database");

      var containerName = "ef-migration-test-sqlserver";
      var saPassword = "TestPassword123!";
      var databaseName = "ConduitMigrationTest";
      var connectionString = $"Server=localhost,1433;Database={databaseName};User Id=sa;Password={saPassword};TrustServerCertificate=True;";

      try
      {
        // Clean up any existing container
        Log.Information("Cleaning up any existing test container...");
        ProcessTasks.StartProcess("docker", $"rm -f {containerName}",
          workingDirectory: RootDirectory,
          logOutput: false,
          logInvocation: false);

        // Start SQL Server container
        Log.Information("Starting SQL Server container...");
        var startArgs = $"run -d --name {containerName} -e \"ACCEPT_EULA=Y\" -e \"SA_PASSWORD={saPassword}\" -p 1433:1433 mcr.microsoft.com/mssql/server:2022-latest";
        ProcessTasks.StartProcess("docker", startArgs,
            workingDirectory: RootDirectory)
          .AssertZeroExitCode();

        // Wait for SQL Server to be ready
        Log.Information("Waiting for SQL Server to be ready (up to 120 seconds)...");

        // Initial wait for container to start
        System.Threading.Thread.Sleep(5000);

        var maxRetries = 60;
        var retryCount = 0;
        var isReady = false;

        while (retryCount < maxRetries && !isReady)
        {
          try
          {
            var result = ProcessTasks.StartProcess("docker",
              $"exec {containerName} /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P {saPassword} -Q \"SELECT 1\" -C",
              workingDirectory: RootDirectory,
              logOutput: false,
              logInvocation: false);

            result.WaitForExit();

            if (result.ExitCode == 0)
            {
              isReady = true;
              Log.Information("✓ SQL Server is ready");
            }
          }
          catch (Exception ex)
          {
            // Ignore errors during health check
            Log.Debug("Health check attempt {Attempt} failed: {Message}", retryCount + 1, ex.Message);
          }

          if (!isReady)
          {
            System.Threading.Thread.Sleep(2000);
            retryCount++;
          }
        }

        if (!isReady)
        {
          throw new Exception("SQL Server failed to start within the timeout period");
        }

        // Apply migrations using dotnet ef
        Log.Information("Applying migrations to test database...");

        // Quote the connection string properly for shell execution
        var quotedConnectionString = $"\\\"{connectionString}\\\"";
        var customArguments = $"ef database update --project {ServerInfrastructureProject} --startup-project {ServerProject} --connection {quotedConnectionString}";
        var efProcess = ProcessTasks.StartProcess(ToolPathResolver.GetPathExecutable("dotnet"),
            customArguments,
            workingDirectory: RootDirectory)
          .AssertZeroExitCode();

        Log.Information("✓ Migrations applied successfully to test database");
      }
      catch (Exception ex)
      {
        Log.Error("Migration test failed: {Message}", ex.Message);
        throw;
      }
      finally
      {
        // Clean up container
        Log.Information("Cleaning up test container...");
        ProcessTasks.StartProcess("docker", $"rm -f {containerName}",
          workingDirectory: RootDirectory,
          logOutput: false,
          logInvocation: false);
        Log.Information("✓ Test container cleaned up");
      }
    });
}
