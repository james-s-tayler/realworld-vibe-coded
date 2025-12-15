namespace Server.FunctionalTests.Infrastructure;

/// <summary>
/// Represents a leased database for a test with automatic cleanup on disposal.
/// </summary>
public class DatabaseLease : IAsyncDisposable
{
  private readonly SqlServerAssemblyFixture _fixture;
  private bool _disposed;

  public string DatabaseName { get; }

  public string ConnectionString { get; }

  internal DatabaseLease(string databaseName, string connectionString, SqlServerAssemblyFixture fixture)
  {
    DatabaseName = databaseName;
    ConnectionString = connectionString;
    _fixture = fixture;
  }

  public async ValueTask DisposeAsync()
  {
    if (_disposed)
    {
      return;
    }

    _disposed = true;
    await _fixture.ReleaseDatabaseAsync(DatabaseName);
  }
}
