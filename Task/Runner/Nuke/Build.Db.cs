using Microsoft.Data.SqlClient;
using Nuke.Common;
using Nuke.Common.Tooling;
using Serilog;

public partial class Build
{
  [Parameter("Force operation without confirmation")]
  readonly bool Force = false;

  Target DbReset => _ => _
    .Description("Reset local database - drops SQL Server schema/data from docker-compose or deletes SQLite file (confirm or --force to skip)")
    .Executes(() =>
    {
      if (!Force)
      {
        Console.Write("Are you sure? This will delete all database data and schema. [y/N] ");
        var response = Console.ReadLine();
        if (response?.ToLowerInvariant() != "y")
        {
          Console.WriteLine("Operation cancelled");
          return;
        }
      }

      ResetDatabase();
    });

  Target DbResetForce => _ => _
    .Description("Reset local database without confirmation - drops SQL Server schema/data from docker-compose or deletes SQLite file")
    .Executes(() =>
    {
      ResetDatabase();
    });

  void ResetDatabase()
  {
    // Check if SQL Server from docker-compose is running
    if (IsSqlServerRunning())
    {
      Log.Information("Detected running SQL Server from docker-compose. Resetting SQL Server database...");
      ResetSqlServerDatabase();
    }
    else
    {
      // Fallback to SQLite reset
      Log.Information("SQL Server not detected. Falling back to SQLite database reset...");
      Console.WriteLine($"Deleting {DatabaseFile}...");
      if (File.Exists(DatabaseFile))
      {
        File.Delete(DatabaseFile);
      }
      Console.WriteLine("Done.");
    }
  }

  bool IsSqlServerRunning()
  {
    try
    {
      var composeFile = RootDirectory / "Task" / "LocalDev" / "docker-compose.yml";
      if (!File.Exists(composeFile))
      {
        return false;
      }

      // Check if docker-compose containers are running
      var psArgs = $"compose -f {composeFile} ps --services --filter status=running";
      var psProcess = ProcessTasks.StartProcess("docker", psArgs,
            workingDirectory: RootDirectory,
            logOutput: false);
      psProcess.WaitForExit();

      var output = string.Join("\n", psProcess.Output.Select(o => o.Text));
      return output.Contains("sqlserver");
    }
    catch (Exception ex)
    {
      Log.Debug("Failed to check if SQL Server is running: {Message}", ex.Message);
      return false;
    }
  }

  void ResetSqlServerDatabase()
  {
    // Connection string from docker-compose.yml
    // Note: When running from host machine (not inside docker), we use localhost:1433
    // The docker-compose.yml API service uses sqlserver:1433 (internal docker network)
    var connectionString = "Server=localhost,1433;Database=Conduit;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;MultipleActiveResultSets=true";
    const int commandTimeoutSeconds = 120;

    try
    {
      using var connection = new SqlConnection(connectionString);
      connection.Open();
      Log.Information("Connected to SQL Server");

      // Drop all user schema objects
      // Note: This uses dynamic SQL in T-SQL to enumerate and drop objects.
      // The QUOTENAME function is used to properly escape object names and prevent SQL injection.
      var dropScript = @"
-- Drop all foreign key constraints first
DECLARE @sql NVARCHAR(MAX) = N'';
SELECT @sql += N'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(parent_object_id)) + '.' 
    + QUOTENAME(OBJECT_NAME(parent_object_id)) + ' DROP CONSTRAINT ' + QUOTENAME(name) + ';'
FROM sys.foreign_keys;
EXEC sp_executesql @sql;

-- Drop all views
SET @sql = N'';
SELECT @sql += N'DROP VIEW ' + QUOTENAME(SCHEMA_NAME(schema_id)) + '.' + QUOTENAME(name) + ';'
FROM sys.views WHERE is_ms_shipped = 0;
EXEC sp_executesql @sql;

-- Drop all tables
SET @sql = N'';
SELECT @sql += N'DROP TABLE ' + QUOTENAME(SCHEMA_NAME(schema_id)) + '.' + QUOTENAME(name) + ';'
FROM sys.tables WHERE is_ms_shipped = 0;
EXEC sp_executesql @sql;

-- Drop all stored procedures
SET @sql = N'';
SELECT @sql += N'DROP PROCEDURE ' + QUOTENAME(SCHEMA_NAME(schema_id)) + '.' + QUOTENAME(name) + ';'
FROM sys.procedures WHERE is_ms_shipped = 0;
EXEC sp_executesql @sql;

-- Drop all functions
SET @sql = N'';
SELECT @sql += N'DROP FUNCTION ' + QUOTENAME(SCHEMA_NAME(schema_id)) + '.' + QUOTENAME(name) + ';'
FROM sys.objects WHERE type IN ('FN', 'IF', 'TF') AND is_ms_shipped = 0;
EXEC sp_executesql @sql;

-- Drop all user-defined types
SET @sql = N'';
SELECT @sql += N'DROP TYPE ' + QUOTENAME(SCHEMA_NAME(schema_id)) + '.' + QUOTENAME(name) + ';'
FROM sys.types WHERE is_user_defined = 1;
EXEC sp_executesql @sql;
";

      using var command = new SqlCommand(dropScript, connection);
      command.CommandTimeout = commandTimeoutSeconds;
      command.ExecuteNonQuery();

      Log.Information("✓ SQL Server database reset complete - all schema and data removed");
    }
    catch (SqlException ex)
    {
      Log.Error("Failed to connect to SQL Server: {Message}", ex.Message);
      Log.Warning("Make sure SQL Server is running via: docker compose -f Task/LocalDev/docker-compose.yml up");
      throw new Exception($"SQL Server reset failed: {ex.Message}");
    }
  }
}
