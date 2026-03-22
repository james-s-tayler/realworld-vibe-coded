using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Docker;
using Serilog;
using Constants = Nuke.Constants;

public partial class Build
{
  [Parameter("Force operation without confirmation")]
  internal readonly bool Force;

  [Parameter("Migration name for DbMigrationsAdd target")]
  internal readonly string? MigrationName;

  internal Target DbReset => _ => _
    .Description("Reset local SQL Server database by removing docker volume (confirm or --force to skip)")
    .Executes(() =>
    {
      if (!Force)
      {
        Console.Write("Are you sure? This will delete local dev database by removing the docker volume. [y/N] ");
        var response = Console.ReadLine();
        if (response?.ToLowerInvariant() != "y")
        {
          Console.WriteLine("Operation cancelled");
          return;
        }
      }

      ResetDatabase();
    });

  internal Target DbResetForce => _ => _
    .Description("Reset local dev database without confirmation by removing the docker volume")
    .Executes(ResetDatabase);

  internal void ResetDatabase()
  {
    if (DoesDockerVolumeExist(Constants.Docker.Volumes.SqlServer))
    {
      Log.Information("Detected SQL Server docker volume. Removing volume to reset database...");
      RemoveSqlServerVolume();
    }
    else
    {
      Log.Information("SQL Server docker volume not found. Nothing to do.");
    }
  }

  internal bool DoesDockerVolumeExist(string volumeName)
  {
    try
    {
      DockerTasks.DockerVolumeInspect(_ => _.SetVolumes(volumeName));
      return true;
    }
    catch
    {
      return false;
    }
  }

  internal void RemoveSqlServerVolume()
  {
    try
    {
      // Stop any running containers first
      Log.Information("Stopping SQL Server container if running...");
      DockerTasks.Docker($"compose -f {DockerComposeDependencies} -p {Constants.Docker.Projects.DevDependencies} down", workingDirectory: RootDirectory);

      // Remove the volume
      Log.Information("Removing SQL Server docker volume...");
      DockerTasks.DockerVolumeRm(_ => _.SetVolumes(Constants.Docker.Volumes.SqlServer));

      Log.Information("✓ SQL Server database reset complete - docker volume removed");
    }
    catch (Exception ex)
    {
      Log.Error("Failed to reset SQL Server database: {Message}", ex.Message);
      throw;
    }
  }

  internal Target DbMigrationsVerifyApply => _ => _
    .Description("Verify EF Core migrations by applying them to a throwaway SQL Server database in Docker")
    .Executes(() =>
    {
      Log.Information("Verifying migrations against a throwaway SQL Server database using Docker Compose");

      var composeFile = RootDirectory / "Test" / "Migrations" / "docker-compose.yml";

      int exitCode = 0;
      try
      {
        // Run docker-compose to start SQL Server and apply migrations
        Log.Information("Running Docker Compose to verify migrations...");
        var args = $"compose -f {composeFile} up --build --abort-on-container-exit";
        var process = ProcessTasks.StartProcess(
              "docker",
              args,
              workingDirectory: RootDirectory);
        process.WaitForExit();
        exitCode = process.ExitCode;
      }
      finally
      {
        // Clean up containers
        Log.Information("Cleaning up Docker Compose resources...");
        var downArgs = $"compose -f {composeFile} down";
        var downProcess = ProcessTasks.StartProcess(
              "docker",
              downArgs,
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
        throw new Exception($"Migration verification failed with exit code: {exitCode}");
      }

      Log.Information("✓ Migrations applied successfully to test database");
    });

  internal Target DbMigrationsGenerateIdempotentScript => _ => _
    .Description("Generate idempotent SQL script from EF Core migrations")
    .DependsOn(InstallDotnetToolEf)
    .Executes(() =>
    {
      Log.Information("Generating idempotent SQL script...");

      var dockerfilePath = RootDirectory / "Test" / "Migrations" / "Dockerfile";

      try
      {
        // Build with build arg for context
        ProcessTasks.StartProcess(
          "docker",
          $"build --target generate-idempotent --build-arg DBCONTEXT=AppDbContext -f {dockerfilePath} -t migrations-generate-appdbcontext {RootDirectory}",
          workingDirectory: RootDirectory,
          logOutput: true)
          .AssertZeroExitCode();

        ProcessTasks.StartProcess(
          "docker",
          $"run --rm -v {MigrationsDirectory}:/output migrations-generate-appdbcontext",
          workingDirectory: RootDirectory,
          logOutput: true)
          .AssertZeroExitCode();

        Log.Information("✓ Idempotent SQL script generated successfully at {ScriptPath}", IdempotentScriptPath);
      }
      catch (Exception ex)
      {
        Log.Error("Failed to generate idempotent SQL script: {Message}", ex.Message);
        throw;
      }
    });

  internal Target DbMigrationsAdd => _ => _
    .Description("Add a new EF Core migration (requires --migration-name parameter)")
    .DependsOn(InstallDotnetToolEf)
    .Triggers(LintServerFix)
    .Executes(() =>
    {
      if (string.IsNullOrWhiteSpace(MigrationName))
      {
        Log.Error("Migration name is required. Use --migration-name <name> parameter");
        throw new Exception("Migration name is required. Use --migration-name <name> parameter");
      }

      Log.Information("Adding new migration: {MigrationName}", MigrationName);

      // Add migration using dotnet ef
      var args = $"ef migrations add {MigrationName} --context AppDbContext --output-dir Data/Migrations --project {ServerInfrastructureProject} --startup-project {ServerProject}";

      try
      {
        ProcessTasks.StartProcess(
          "dotnet",
          args,
          workingDirectory: RootDirectory,
          logOutput: true)
          .AssertZeroExitCode();

        Log.Information("✓ Migration '{MigrationName}' added successfully", MigrationName);
        Log.Information("Don't forget to run 'nuke DbMigrationsGenerateIdempotentScript' to update the idempotent script");
      }
      catch (Exception ex)
      {
        Log.Error("Failed to add migration: {Message}", ex.Message);
        throw;
      }
    });

  internal Target DbMigrationsVerifyIdempotentScript => _ => _
    .Description("Verify that the idempotent SQL script matches the current migrations")
    .DependsOn(InstallDotnetToolEf)
    .Executes(() =>
    {
      Log.Information("Verifying idempotent SQL script is up to date...");

      if (!IdempotentScriptPath.FileExists())
      {
        Log.Error("Idempotent SQL script not found at {ScriptPath}", IdempotentScriptPath);
        Log.Error("Run 'nuke DbMigrationsGenerateIdempotentScript' to generate the script and commit it to source control.");
        throw new Exception("Idempotent SQL script not found in source control");
      }

      var dockerfilePath = RootDirectory / "Test" / "Migrations" / "Dockerfile";

      try
      {
        ProcessTasks.StartProcess(
          "docker",
          $"build --target verify-idempotent --build-arg DBCONTEXT=AppDbContext -f {dockerfilePath} -t migrations-verify-appdbcontext {RootDirectory}",
          workingDirectory: RootDirectory,
          logOutput: true)
          .AssertZeroExitCode();

        ProcessTasks.StartProcess(
          "docker",
          $"run --rm -v {MigrationsDirectory}:/committed migrations-verify-appdbcontext",
          workingDirectory: RootDirectory,
          logOutput: true)
          .AssertZeroExitCode();

        Log.Information("✓ Idempotent SQL script is up to date with current migrations");
      }
      catch (Exception ex)
      {
        Log.Error("Idempotent SQL script verification failed: {Message}", ex.Message);
        throw;
      }
    });

  internal Target DbMigrationsVerifyAll => _ => _
    .Description("Verify all database migrations: apply to test database and verify idempotent SQL scripts")
    .DependsOn(DbMigrationsVerifyApply, InstallDotnetToolEf)
    .Executes(() =>
    {
      // Verify AppDbContext
      Log.Information("Verifying idempotent SQL script is up to date...");

      if (!IdempotentScriptPath.FileExists())
      {
        Log.Error("Idempotent SQL script not found at {ScriptPath}", IdempotentScriptPath);
        Log.Error("Run 'nuke DbMigrationsGenerateIdempotentScript' to generate the script and commit it to source control.");
        throw new Exception("Idempotent SQL script not found in source control");
      }

      var dockerfilePath = RootDirectory / "Test" / "Migrations" / "Dockerfile";

      try
      {
        ProcessTasks.StartProcess(
          "docker",
          $"build --target verify-idempotent --build-arg DBCONTEXT=AppDbContext -f {dockerfilePath} -t migrations-verify-appdbcontext {RootDirectory}",
          workingDirectory: RootDirectory,
          logOutput: true)
          .AssertZeroExitCode();

        ProcessTasks.StartProcess(
          "docker",
          $"run --rm -v {MigrationsDirectory}:/committed migrations-verify-appdbcontext",
          workingDirectory: RootDirectory,
          logOutput: true)
          .AssertZeroExitCode();

        Log.Information("✓ Idempotent SQL script is up to date with current migrations");
      }
      catch (Exception ex)
      {
        Log.Error("Idempotent SQL script verification failed: {Message}", ex.Message);
        throw;
      }

      Log.Information("✓ All database migration verifications completed successfully");
    });
}
