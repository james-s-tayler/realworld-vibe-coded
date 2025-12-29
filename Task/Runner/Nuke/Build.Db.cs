using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Docker;
using Serilog;

public partial class Build
{
  [Parameter("Force operation without confirmation")]
  internal readonly bool Force;

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

    if (DoesDockerVolumeExist("localdev_sqlserver-data"))
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
      DockerTasks.Docker($"compose -f {composeFile} down", workingDirectory: RootDirectory);

      // Remove the volume
      Log.Information("Removing SQL Server docker volume...");
      DockerTasks.DockerVolumeRm(_ => _
        .SetVolumes("localdev_sqlserver-data"));

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
    .DependsOn(BuildServer)
    .Executes(() =>
    {
      Log.Information("Generating idempotent SQL scripts from migrations...");

      // Generate idempotent script for AppDbContext
      var appDbContextArgs = $"ef migrations script --idempotent --project {ServerInfrastructureProject} --startup-project {ServerProject} --context AppDbContext --output {IdempotentScriptPath}";

      try
      {
        ProcessTasks.StartProcess(
          "dotnet",
          appDbContextArgs,
          workingDirectory: RootDirectory,
          logOutput: true)
          .AssertZeroExitCode();

        Log.Information("✓ AppDbContext idempotent SQL script generated successfully at {ScriptPath}", IdempotentScriptPath);

        // Generate idempotent script for TenantStoreDbContext
        var tenantStoreScriptPath = RootDirectory / "App" / "Server" / "src" / "Server.Infrastructure" / "Data" / "Migrations" / "TenantStoreDbContext" / "idempotent.sql";
        var tenantStoreArgs = $"ef migrations script --idempotent --project {ServerInfrastructureProject} --startup-project {ServerProject} --context TenantStoreDbContext --output {tenantStoreScriptPath}";

        ProcessTasks.StartProcess(
          "dotnet",
          tenantStoreArgs,
          workingDirectory: RootDirectory,
          logOutput: true)
          .AssertZeroExitCode();

        Log.Information("✓ TenantStoreDbContext idempotent SQL script generated successfully at {ScriptPath}", tenantStoreScriptPath);
      }
      catch (Exception ex)
      {
        Log.Error("Failed to generate idempotent SQL scripts: {Message}", ex.Message);
        throw;
      }
    });

  internal Target DbMigrationsVerifyIdempotentScript => _ => _
    .Description("Verify that the idempotent SQL script matches the current migrations")
    .DependsOn(InstallDotnetToolEf)
    .DependsOn(BuildServer)
    .Executes(() =>
    {
      Log.Information("Verifying idempotent SQL scripts are up to date...");

      // Verify AppDbContext script
      if (!IdempotentScriptPath.FileExists())
      {
        Log.Error("AppDbContext idempotent SQL script not found at {ScriptPath}", IdempotentScriptPath);
        Log.Error("Run 'nuke DbMigrationsGenerateIdempotentScript' to generate the script and commit it to source control.");
        throw new Exception("AppDbContext idempotent SQL script not found in source control");
      }

      var committedAppScript = IdempotentScriptPath.ReadAllText();
      var tempAppScriptPath = RootDirectory / "temp-idempotent-app.sql";
      var appArgs = $"ef migrations script --idempotent --project {ServerInfrastructureProject} --startup-project {ServerProject} --context AppDbContext --output {tempAppScriptPath}";

      try
      {
        ProcessTasks.StartProcess(
          "dotnet",
          appArgs,
          workingDirectory: RootDirectory,
          logOutput: false,
          logInvocation: false)
          .AssertZeroExitCode();

        var generatedAppScript = tempAppScriptPath.ReadAllText();
        tempAppScriptPath.DeleteFile();

        if (committedAppScript != generatedAppScript)
        {
          Log.Error("AppDbContext idempotent SQL script is out of sync with current migrations!");
          Log.Error("The committed script at {ScriptPath} does not match the script generated from current migrations.", IdempotentScriptPath);
          Log.Error("Run 'nuke DbMigrationsGenerateIdempotentScript' to regenerate the script and commit the changes to source control.");
          throw new Exception("AppDbContext idempotent SQL script is out of sync with migrations");
        }

        Log.Information("✓ AppDbContext idempotent SQL script is up to date with current migrations");

        // Verify TenantStoreDbContext script
        var tenantStoreScriptPath = RootDirectory / "App" / "Server" / "src" / "Server.Infrastructure" / "Data" / "Migrations" / "TenantStoreDbContext" / "idempotent.sql";

        if (!tenantStoreScriptPath.FileExists())
        {
          Log.Error("TenantStoreDbContext idempotent SQL script not found at {ScriptPath}", tenantStoreScriptPath);
          Log.Error("Run 'nuke DbMigrationsGenerateIdempotentScript' to generate the script and commit it to source control.");
          throw new Exception("TenantStoreDbContext idempotent SQL script not found in source control");
        }

        var committedTenantScript = tenantStoreScriptPath.ReadAllText();
        var tempTenantScriptPath = RootDirectory / "temp-idempotent-tenant.sql";
        var tenantArgs = $"ef migrations script --idempotent --project {ServerInfrastructureProject} --startup-project {ServerProject} --context TenantStoreDbContext --output {tempTenantScriptPath}";

        ProcessTasks.StartProcess(
          "dotnet",
          tenantArgs,
          workingDirectory: RootDirectory,
          logOutput: false,
          logInvocation: false)
          .AssertZeroExitCode();

        var generatedTenantScript = tempTenantScriptPath.ReadAllText();
        tempTenantScriptPath.DeleteFile();

        if (committedTenantScript != generatedTenantScript)
        {
          Log.Error("TenantStoreDbContext idempotent SQL script is out of sync with current migrations!");
          Log.Error("The committed script at {ScriptPath} does not match the script generated from current migrations.", tenantStoreScriptPath);
          Log.Error("Run 'nuke DbMigrationsGenerateIdempotentScript' to regenerate the script and commit the changes to source control.");
          throw new Exception("TenantStoreDbContext idempotent SQL script is out of sync with migrations");
        }

        Log.Information("✓ TenantStoreDbContext idempotent SQL script is up to date with current migrations");
        Log.Information("✓ All idempotent SQL scripts are up to date with current migrations");
      }
      catch (Exception ex)
      {
        // Clean up temp files if they exist
        if (tempAppScriptPath.FileExists())
        {
          tempAppScriptPath.DeleteFile();
        }

        var tempTenantScriptPath = RootDirectory / "temp-idempotent-tenant.sql";
        if (tempTenantScriptPath.FileExists())
        {
          tempTenantScriptPath.DeleteFile();
        }

        if (ex.Message.Contains("out of sync"))
        {
          throw;
        }

        Log.Error("Failed to verify idempotent SQL scripts: {Message}", ex.Message);
        throw;
      }
    });

  internal Target DbMigrationsVerifyAll => _ => _
    .Description("Verify all database migrations: apply to test database and verify idempotent SQL script")
    .DependsOn(DbMigrationsVerifyApply, DbMigrationsVerifyIdempotentScript)
    .Executes(() =>
    {
      Log.Information("✓ All database migration verifications completed successfully");
    });
}
