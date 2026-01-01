using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Docker;
using Serilog;

public partial class Build
{
  [Parameter("Force operation without confirmation")]
  internal readonly bool Force;

  [Parameter("Migration name for DbMigrationsAdd target")]
  internal readonly string? MigrationName;

  [Parameter("DbContext name for migrations (e.g., AppDbContext, TenantStoreDbContext)")]
  internal readonly string? DbContext;

  internal AbsolutePath GetMigrationsDirectory(string? dbContext)
  {
    return dbContext?.ToLowerInvariant() switch
    {
      "tenantstoredbcontext" => TenantStoreMigrationsDirectory,
      "appdbcontext" => MigrationsDirectory,
      _ => MigrationsDirectory, // Default to AppDbContext
    };
  }

  internal AbsolutePath GetIdempotentScriptPath(string? dbContext)
  {
    return dbContext?.ToLowerInvariant() switch
    {
      "tenantstoredbcontext" => TenantStoreIdempotentScriptPath,
      "appdbcontext" => IdempotentScriptPath,
      _ => IdempotentScriptPath, // Default to AppDbContext
    };
  }

  internal string GetContextFlag(string? dbContext)
  {
    return string.IsNullOrWhiteSpace(dbContext) ? string.Empty : $"--context {dbContext}";
  }

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
    var composeFile = TaskLocalDevDirectory / "docker-compose.dev-deps.yml";

    if (DoesDockerVolumeExist("dev-dependencies_sqlserver-data"))
    {
      Log.Information("Detected SQL Server docker volume. Removing volume to reset database...");
      RemoveSqlServerVolume(composeFile);
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
      DockerTasks.DockerVolumeInspect(_ => _
        .SetVolumes(volumeName));
      return true;
    }
    catch
    {
      return false;
    }
  }

  internal void RemoveSqlServerVolume(string composeFile)
  {
    try
    {
      // Stop any running containers first
      Log.Information("Stopping SQL Server container if running...");
      DockerTasks.Docker($"compose -f {composeFile} -p dev-dependencies down", workingDirectory: RootDirectory);

      // Remove the volume
      Log.Information("Removing SQL Server docker volume...");
      DockerTasks.DockerVolumeRm(_ => _
        .SetVolumes("dev-dependencies_sqlserver-data"));

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
    .Description("Generate idempotent SQL script from EF Core migrations (use --db-context to specify context)")
    .DependsOn(InstallDotnetToolEf)
    .Executes(() =>
    {
      var contextName = DbContext ?? "AppDbContext";
      var outputDir = GetMigrationsDirectory(contextName);
      var scriptPath = GetIdempotentScriptPath(contextName);

      Log.Information("Generating idempotent SQL script for {DbContext}...", contextName);

      var dockerfilePath = RootDirectory / "Test" / "Migrations" / "Dockerfile";

      try
      {
        // Build with build arg for context
        ProcessTasks.StartProcess(
          "docker",
          $"build --target generate-idempotent --build-arg DBCONTEXT={contextName} -f {dockerfilePath} -t migrations-generate-{contextName.ToLowerInvariant()} {RootDirectory}",
          workingDirectory: RootDirectory,
          logOutput: true)
          .AssertZeroExitCode();

        ProcessTasks.StartProcess(
          "docker",
          $"run --rm -v {outputDir}:/output migrations-generate-{contextName.ToLowerInvariant()}",
          workingDirectory: RootDirectory,
          logOutput: true)
          .AssertZeroExitCode();

        Log.Information("✓ Idempotent SQL script generated successfully for {DbContext} at {ScriptPath}", contextName, scriptPath);
      }
      catch (Exception ex)
      {
        Log.Error("Failed to generate idempotent SQL script for {DbContext}: {Message}", contextName, ex.Message);
        throw;
      }
    });

  internal Target DbMigrationsAdd => _ => _
    .Description("Add a new EF Core migration (requires --migration-name and --db-context parameters)")
    .DependsOn(InstallDotnetToolEf)
    .Executes(() =>
    {
      if (string.IsNullOrWhiteSpace(MigrationName))
      {
        Log.Error("Migration name is required. Use --migration-name <name> parameter");
        throw new Exception("Migration name is required. Use --migration-name <name> parameter");
      }

      if (string.IsNullOrWhiteSpace(DbContext))
      {
        Log.Error("DbContext name is required. Use --db-context <context> parameter (e.g., AppDbContext or TenantStoreDbContext)");
        throw new Exception("DbContext name is required. Use --db-context <context> parameter");
      }

      Log.Information("Adding new migration: {MigrationName} for {DbContext}", MigrationName, DbContext);

      // Determine output directory based on context
      var outputDir = DbContext.ToLowerInvariant() == "tenantstoredbcontext"
        ? "Data/TenantStoreMigrations"
        : "Data/Migrations";

      // Add migration using dotnet ef
      var args = $"ef migrations add {MigrationName} --context {DbContext} --output-dir {outputDir} --project {ServerInfrastructureProject} --startup-project {ServerProject}";

      try
      {
        ProcessTasks.StartProcess(
          "dotnet",
          args,
          workingDirectory: RootDirectory,
          logOutput: true)
          .AssertZeroExitCode();

        Log.Information("✓ Migration '{MigrationName}' added successfully for {DbContext}", MigrationName, DbContext);
        Log.Information("Don't forget to run 'nuke DbMigrationsGenerateIdempotentScript --db-context {DbContext}' to update the idempotent script", DbContext);
      }
      catch (Exception ex)
      {
        Log.Error("Failed to add migration: {Message}", ex.Message);
        throw;
      }
    });

  internal Target DbMigrationsVerifyIdempotentScript => _ => _
    .Description("Verify that the idempotent SQL script matches the current migrations (use --db-context to specify context)")
    .DependsOn(InstallDotnetToolEf)
    .Executes(() =>
    {
      var contextName = DbContext ?? "AppDbContext";
      var scriptPath = GetIdempotentScriptPath(contextName);
      var committedScriptDir = GetMigrationsDirectory(contextName);

      Log.Information("Verifying idempotent SQL script for {DbContext} is up to date...", contextName);

      if (!scriptPath.FileExists())
      {
        Log.Error("Idempotent SQL script not found for {DbContext} at {ScriptPath}", contextName, scriptPath);
        Log.Error("Run 'nuke DbMigrationsGenerateIdempotentScript --db-context {DbContext}' to generate the script and commit it to source control.", contextName);
        throw new Exception($"Idempotent SQL script not found for {contextName} in source control");
      }

      var dockerfilePath = RootDirectory / "Test" / "Migrations" / "Dockerfile";

      try
      {
        ProcessTasks.StartProcess(
          "docker",
          $"build --target verify-idempotent --build-arg DBCONTEXT={contextName} -f {dockerfilePath} -t migrations-verify-{contextName.ToLowerInvariant()} {RootDirectory}",
          workingDirectory: RootDirectory,
          logOutput: true)
          .AssertZeroExitCode();

        ProcessTasks.StartProcess(
          "docker",
          $"run --rm -v {committedScriptDir}:/committed migrations-verify-{contextName.ToLowerInvariant()}",
          workingDirectory: RootDirectory,
          logOutput: true)
          .AssertZeroExitCode();

        Log.Information("✓ Idempotent SQL script for {DbContext} is up to date with current migrations", contextName);
      }
      catch (Exception ex)
      {
        Log.Error("Idempotent SQL script verification failed for {DbContext}: {Message}", contextName, ex.Message);
        throw;
      }
    });

  internal Target DbMigrationsVerifyAll => _ => _
    .Description("Verify all database migrations for all DbContexts: apply to test database and verify idempotent SQL scripts")
    .DependsOn(DbMigrationsVerifyApply, InstallDotnetToolEf)
    .Executes(() =>
    {
      // Verify AppDbContext
      VerifyIdempotentScriptForContext("AppDbContext");

      // Verify TenantStoreDbContext
      VerifyIdempotentScriptForContext("TenantStoreDbContext");

      Log.Information("✓ All database migration verifications completed successfully");
    });

  private void VerifyIdempotentScriptForContext(string contextName)
  {
    var scriptPath = GetIdempotentScriptPath(contextName);
    var committedScriptDir = GetMigrationsDirectory(contextName);

    Log.Information("Verifying idempotent SQL script for {DbContext} is up to date...", contextName);

    if (!scriptPath.FileExists())
    {
      Log.Error("Idempotent SQL script not found for {DbContext} at {ScriptPath}", contextName, scriptPath);
      Log.Error("Run 'nuke DbMigrationsGenerateIdempotentScript --db-context {DbContext}' to generate the script and commit it to source control.", contextName);
      throw new Exception($"Idempotent SQL script not found for {contextName} in source control");
    }

    var dockerfilePath = RootDirectory / "Test" / "Migrations" / "Dockerfile";

    try
    {
      ProcessTasks.StartProcess(
        "docker",
        $"build --target verify-idempotent --build-arg DBCONTEXT={contextName} -f {dockerfilePath} -t migrations-verify-{contextName.ToLowerInvariant()} {RootDirectory}",
        workingDirectory: RootDirectory,
        logOutput: true)
        .AssertZeroExitCode();

      ProcessTasks.StartProcess(
        "docker",
        $"run --rm -v {committedScriptDir}:/committed migrations-verify-{contextName.ToLowerInvariant()}",
        workingDirectory: RootDirectory,
        logOutput: true)
        .AssertZeroExitCode();

      Log.Information("✓ Idempotent SQL script for {DbContext} is up to date with current migrations", contextName);
    }
    catch (Exception ex)
    {
      Log.Error("Idempotent SQL script verification failed for {DbContext}: {Message}", contextName, ex.Message);
      throw;
    }
  }
}
