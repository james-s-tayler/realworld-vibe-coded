using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Server.Infrastructure.Data;
using Testcontainers.MsSql;

namespace Server.FunctionalTests.Infrastructure;

/// <summary>
/// Assembly-scoped fixture that manages a single SQL Server container and template database
/// with backup/restore capabilities for per-test database isolation.
/// </summary>
public class SqlServerAssemblyFixture : IAsyncLifetime
{
  private const string TemplateDbName = "TemplateDb";
  private const string BackupFileName = "template.bak";
  private const string BackupDirectory = "/var/opt/mssql/backups";
  private const string SeedVersion = "v1"; // Increment when seed data changes

  private MsSqlContainer? _container;
  private string? _masterConnectionString;
  private string? _schemaVersion;
  private readonly SemaphoreSlim _backupLock = new(1, 1);

  public string MasterConnectionString => _masterConnectionString
    ?? throw new InvalidOperationException("Container not initialized");

  public string SchemaVersion => _schemaVersion
    ?? throw new InvalidOperationException("Schema version not computed");

  public async ValueTask InitializeAsync()
  {
    Console.WriteLine("[SqlServerAssemblyFixture] Starting SQL Server container...");

    // Ensure host directory exists before starting container with proper permissions
    var backupDir = "/tmp/sqlserver-backups";
    Directory.CreateDirectory(backupDir);

    // Set permissions to 777 so SQL Server container can write to it
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
        RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
    {
      var process = new System.Diagnostics.Process
      {
        StartInfo = new System.Diagnostics.ProcessStartInfo
        {
          FileName = "chmod",
          Arguments = $"777 {backupDir}",
          RedirectStandardOutput = true,
          RedirectStandardError = true,
          UseShellExecute = false,
          CreateNoWindow = true,
        },
      };
      process.Start();
      await process.WaitForExitAsync();
    }

    _container = new MsSqlBuilder()
      .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
      .WithBindMount(backupDir, BackupDirectory)
      .Build();

    await _container.StartAsync();
    _masterConnectionString = _container.GetConnectionString();

    Console.WriteLine($"[SqlServerAssemblyFixture] Container started. Connection string: {MaskConnectionString(_masterConnectionString)}");

    // Ensure backup directory exists
    await EnsureBackupDirectoryExistsAsync();

    // Compute schema version
    _schemaVersion = await ComputeSchemaVersionAsync();
    Console.WriteLine($"[SqlServerAssemblyFixture] Schema version: {_schemaVersion}");

    // Ensure template backup exists and is valid
    await EnsureTemplateBackupAsync();
  }

  public async ValueTask DisposeAsync()
  {
    Console.WriteLine("[SqlServerAssemblyFixture] Disposing container...");

    if (_container != null)
    {
      await _container.DisposeAsync();
    }

    _backupLock.Dispose();
  }

  /// <summary>
  /// Lease a new database for a test by restoring from template backup.
  /// </summary>
  public async Task<DatabaseLease> LeaseDatabaseAsync(CancellationToken cancellationToken = default)
  {
    var dbName = $"TestDb_{Guid.NewGuid():N}";
    Console.WriteLine($"[SqlServerAssemblyFixture] Leasing database: {dbName}");

    await RestoreDatabaseFromBackupAsync(dbName, cancellationToken);

    var connectionString = BuildConnectionString(dbName);
    return new DatabaseLease(dbName, connectionString, this);
  }

  /// <summary>
  /// Release a database lease by dropping the database.
  /// </summary>
  internal async Task ReleaseDatabaseAsync(string dbName, CancellationToken cancellationToken = default)
  {
    try
    {
      Console.WriteLine($"[SqlServerAssemblyFixture] Releasing database: {dbName}");

      await using var connection = new SqlConnection(MasterConnectionString);
      await connection.OpenAsync(cancellationToken);

      // Set database to single user mode to force disconnect all sessions
      var setSingleUserSql = $@"
        IF EXISTS (SELECT 1 FROM sys.databases WHERE name = '{dbName}')
        BEGIN
          ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
          DROP DATABASE [{dbName}];
        END";

      await using var command = new SqlCommand(setSingleUserSql, connection);
      command.CommandTimeout = 30;
      await command.ExecuteNonQueryAsync(cancellationToken);

      Console.WriteLine($"[SqlServerAssemblyFixture] Database {dbName} dropped successfully");
    }
    catch (Exception ex)
    {
      Console.WriteLine($"[SqlServerAssemblyFixture] Warning: Failed to drop database {dbName}: {ex.Message}");

      // Don't throw - cleanup is best-effort
    }
  }

  private static string MaskConnectionString(string connectionString)
  {
    var builder = new SqlConnectionStringBuilder(connectionString);
    if (!string.IsNullOrEmpty(builder.Password))
    {
      builder.Password = "****";
    }

    return builder.ToString();
  }

  private string BuildConnectionString(string dbName)
  {
    var builder = new SqlConnectionStringBuilder(MasterConnectionString)
    {
      InitialCatalog = dbName,
    };

    return builder.ToString();
  }

  private async Task EnsureBackupDirectoryExistsAsync()
  {
    // Ensure host directory exists
    Directory.CreateDirectory("/tmp/sqlserver-backups");

    // Verify container can access backup directory
    await using var connection = new SqlConnection(MasterConnectionString);
    await connection.OpenAsync();

    var sql = $@"
      EXEC xp_create_subdir '{BackupDirectory}';
    ";

    await using var command = new SqlCommand(sql, connection);
    command.CommandTimeout = 30;
    await command.ExecuteNonQueryAsync();

    Console.WriteLine($"[SqlServerAssemblyFixture] Backup directory ensured: {BackupDirectory}");
  }

  private async Task<string> ComputeSchemaVersionAsync()
  {
    // Find all migration files
    var migrationsPath = Path.Combine(
      AppDomain.CurrentDomain.BaseDirectory,
      "..",
      "..",
      "..",
      "..",
      "..",
      "src",
      "Server.Infrastructure",
      "Data",
      "Migrations");

    var fullPath = Path.GetFullPath(migrationsPath);

    if (!Directory.Exists(fullPath))
    {
      throw new DirectoryNotFoundException($"Migrations directory not found: {fullPath}");
    }

    var migrationFiles = Directory.GetFiles(fullPath, "*.cs")
      .Where(f => !f.EndsWith("ModelSnapshot.cs") && !f.EndsWith("Designer.cs"))
      .OrderBy(f => f)
      .ToList();

    // Compute hash of all migration file contents + seed version
    using var sha256 = SHA256.Create();
    var combinedContent = new StringBuilder();

    foreach (var file in migrationFiles)
    {
      var content = await File.ReadAllTextAsync(file);
      combinedContent.Append(content);
    }

    combinedContent.Append(SeedVersion);

    var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(combinedContent.ToString()));
    return Convert.ToHexString(hash)[..16]; // Use first 16 chars of hash
  }

  private async Task EnsureTemplateBackupAsync()
  {
    var backupPath = $"{BackupDirectory}/{BackupFileName}";
    var schemaVersionFile = $"{BackupDirectory}/schema_version.txt";

    await _backupLock.WaitAsync();
    try
    {
      // Check if backup exists and has matching schema version
      var backupExists = await BackupExistsAsync(backupPath);
      var schemaMatches = false;

      if (backupExists && File.Exists($"/tmp/sqlserver-backups/schema_version.txt"))
      {
        var existingVersion = await File.ReadAllTextAsync($"/tmp/sqlserver-backups/schema_version.txt");
        schemaMatches = existingVersion.Trim() == SchemaVersion;
      }

      if (backupExists && schemaMatches)
      {
        Console.WriteLine($"[SqlServerAssemblyFixture] Template backup exists and schema matches. Reusing backup.");
        return;
      }

      if (backupExists && !schemaMatches)
      {
        Console.WriteLine($"[SqlServerAssemblyFixture] Schema version changed. Recreating template backup...");
      }
      else
      {
        Console.WriteLine($"[SqlServerAssemblyFixture] Template backup does not exist. Creating...");
      }

      // Build template database
      await BuildTemplateDbAsync();

      // Backup template database
      await BackupTemplateAsync(backupPath);

      // Save schema version
      await File.WriteAllTextAsync($"/tmp/sqlserver-backups/schema_version.txt", SchemaVersion);

      Console.WriteLine($"[SqlServerAssemblyFixture] Template backup created successfully.");
    }
    finally
    {
      _backupLock.Release();
    }
  }

  private async Task<bool> BackupExistsAsync(string backupPath)
  {
    try
    {
      await using var connection = new SqlConnection(MasterConnectionString);
      await connection.OpenAsync();

      var sql = $@"
        DECLARE @FileExists INT;
        EXEC xp_fileexist '{backupPath}', @FileExists OUTPUT;
        SELECT @FileExists;
      ";

      await using var command = new SqlCommand(sql, connection);
      command.CommandTimeout = 30;
      var result = await command.ExecuteScalarAsync();
      return result != null && Convert.ToInt32(result) == 1;
    }
    catch
    {
      // If we can't check, assume it doesn't exist
      return false;
    }
  }

  private async Task BuildTemplateDbAsync()
  {
    Console.WriteLine($"[SqlServerAssemblyFixture] Building template database: {TemplateDbName}");

    // Drop existing template DB if it exists
    await DropDatabaseIfExistsAsync(TemplateDbName);

    // Create template database
    await CreateDatabaseAsync(TemplateDbName);

    // Apply migrations
    var connectionString = BuildConnectionString(TemplateDbName);
    var serviceCollection = new ServiceCollection();
    serviceCollection.AddDbContext<AppDbContext>(options =>
    {
      options.UseSqlServer(connectionString);
      options.EnableSensitiveDataLogging();
    });

    await using var serviceProvider = serviceCollection.BuildServiceProvider();
    var dbContextOptions = serviceProvider.GetRequiredService<DbContextOptions<AppDbContext>>();
    await using var dbContext = new AppDbContext(dbContextOptions, null);

    await dbContext.Database.MigrateAsync();

    // Run seed data
    await SeedData.InitializeAsync(dbContext);

    Console.WriteLine($"[SqlServerAssemblyFixture] Template database built successfully.");
  }

  private async Task BackupTemplateAsync(string backupPath)
  {
    Console.WriteLine($"[SqlServerAssemblyFixture] Backing up template database to: {backupPath}");

    await using var connection = new SqlConnection(MasterConnectionString);
    await connection.OpenAsync();

    var sql = $@"
      BACKUP DATABASE [{TemplateDbName}] 
      TO DISK = '{backupPath}' 
      WITH FORMAT, INIT, COMPRESSION;
    ";

    await using var command = new SqlCommand(sql, connection);
    command.CommandTimeout = 120; // Backup can take longer
    await command.ExecuteNonQueryAsync();

    Console.WriteLine($"[SqlServerAssemblyFixture] Backup completed successfully.");
  }

  private async Task RestoreDatabaseFromBackupAsync(string dbName, CancellationToken cancellationToken)
  {
    var backupPath = $"{BackupDirectory}/{BackupFileName}";

    await using var connection = new SqlConnection(MasterConnectionString);
    await connection.OpenAsync(cancellationToken);

    // Get logical file names from backup
    var getFilesSql = $"RESTORE FILELISTONLY FROM DISK = '{backupPath}';";
    await using var getFilesCommand = new SqlCommand(getFilesSql, connection);
    getFilesCommand.CommandTimeout = 30;

    string? dataLogicalName = null;
    string? logLogicalName = null;

    await using (var reader = await getFilesCommand.ExecuteReaderAsync(cancellationToken))
    {
      while (await reader.ReadAsync(cancellationToken))
      {
        var logicalName = reader.GetString(0);
        var type = reader.GetString(2);

        if (type == "D")
        {
          dataLogicalName = logicalName;
        }
        else if (type == "L")
        {
          logLogicalName = logicalName;
        }
      }
    }

    if (dataLogicalName == null || logLogicalName == null)
    {
      throw new InvalidOperationException("Could not determine logical file names from backup");
    }

    // Restore database
    var restoreSql = $@"
      RESTORE DATABASE [{dbName}]
      FROM DISK = '{backupPath}'
      WITH 
        MOVE '{dataLogicalName}' TO '/var/opt/mssql/data/{dbName}.mdf',
        MOVE '{logLogicalName}' TO '/var/opt/mssql/data/{dbName}_log.ldf',
        REPLACE;
    ";

    await using var restoreCommand = new SqlCommand(restoreSql, connection);
    restoreCommand.CommandTimeout = 60; // Restore can take time
    await restoreCommand.ExecuteNonQueryAsync(cancellationToken);

    Console.WriteLine($"[SqlServerAssemblyFixture] Database {dbName} restored from backup.");
  }

  private async Task CreateDatabaseAsync(string dbName)
  {
    await using var connection = new SqlConnection(MasterConnectionString);
    await connection.OpenAsync();

    var sql = $"CREATE DATABASE [{dbName}];";
    await using var command = new SqlCommand(sql, connection);
    command.CommandTimeout = 30;
    await command.ExecuteNonQueryAsync();
  }

  private async Task DropDatabaseIfExistsAsync(string dbName)
  {
    try
    {
      await using var connection = new SqlConnection(MasterConnectionString);
      await connection.OpenAsync();

      var sql = $@"
        IF EXISTS (SELECT 1 FROM sys.databases WHERE name = '{dbName}')
        BEGIN
          ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
          DROP DATABASE [{dbName}];
        END
      ";

      await using var command = new SqlCommand(sql, connection);
      command.CommandTimeout = 30;
      await command.ExecuteNonQueryAsync();
    }
    catch (Exception ex)
    {
      Console.WriteLine($"[SqlServerAssemblyFixture] Warning: Failed to drop database {dbName}: {ex.Message}");
    }
  }
}
