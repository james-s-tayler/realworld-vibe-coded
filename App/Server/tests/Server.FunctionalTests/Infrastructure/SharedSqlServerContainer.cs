using Testcontainers.MsSql;

namespace Server.FunctionalTests.Infrastructure;

/// <summary>
/// Shared SQL Server container manager - single container for all tests.
/// Uses static singleton pattern to ensure single container instance across all fixtures.
/// </summary>
public static class SharedSqlServerContainer
{
  private static readonly SemaphoreSlim InitializationLock = new(1, 1);
  private static MsSqlContainer? _container;
  private static string? _connectionString;
  private static bool _isInitialized;

  /// <summary>
  /// Gets the connection string to the shared SQL Server container.
  /// Initializes the container on first access.
  /// </summary>
  public static async Task<string> GetConnectionStringAsync()
  {
    if (_isInitialized && _connectionString != null)
    {
      return _connectionString;
    }

    await InitializationLock.WaitAsync();
    try
    {
      if (_isInitialized && _connectionString != null)
      {
        return _connectionString;
      }

      Console.WriteLine("[SharedSqlServerContainer] Initializing SQL Server container...");

      _container = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

      await _container.StartAsync();
      _connectionString = _container.GetConnectionString();
      _isInitialized = true;

      Console.WriteLine($"[SharedSqlServerContainer] Container started. Connection string obtained.");

      return _connectionString;
    }
    finally
    {
      InitializationLock.Release();
    }
  }

  /// <summary>
  /// Cleanup method - called at end of test run.
  /// Note: Testcontainers automatically disposes containers, but this can be used for explicit cleanup.
  /// </summary>
  public static async Task DisposeAsync()
  {
    if (_container != null)
    {
      Console.WriteLine("[SharedSqlServerContainer] Disposing container...");
      await _container.DisposeAsync();
      _container = null;
      _connectionString = null;
      _isInitialized = false;
    }
  }
}
