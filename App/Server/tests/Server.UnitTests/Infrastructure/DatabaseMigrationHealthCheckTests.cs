using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Server.Web.Infrastructure;

namespace Server.UnitTests.Infrastructure;

public class DatabaseMigrationHealthCheckTests
{
  private readonly IMemoryCache _cache;
  private readonly IDatabaseMigrationChecker _migrationChecker;

  public DatabaseMigrationHealthCheckTests()
  {
    _cache = new MemoryCache(new MemoryCacheOptions());
    _migrationChecker = Substitute.For<IDatabaseMigrationChecker>();
  }

  [Fact]
  public async Task ReturnsUnhealthy_WhenCannotConnect()
  {
    // Arrange
    _migrationChecker.CanConnectAsync(Arg.Any<CancellationToken>()).Returns(false);
    var healthCheck = new DatabaseMigrationHealthCheck(_migrationChecker, _cache);

    // Act
    var result = await healthCheck.CheckHealthAsync(null!, CancellationToken.None);

    // Assert
    Assert.Equal(HealthStatus.Unhealthy, result.Status);
    Assert.Equal("Cannot connect to database.", result.Description);
  }

  [Fact]
  public async Task ReturnsUnhealthy_WhenPendingMigrationsExist()
  {
    // Arrange
    _migrationChecker.CanConnectAsync(Arg.Any<CancellationToken>()).Returns(true);
    _migrationChecker.GetPendingMigrationsAsync(Arg.Any<CancellationToken>())
      .Returns(new[] { "Migration1", "Migration2" });
    var healthCheck = new DatabaseMigrationHealthCheck(_migrationChecker, _cache);

    // Act
    var result = await healthCheck.CheckHealthAsync(null!, CancellationToken.None);

    // Assert
    Assert.Equal(HealthStatus.Unhealthy, result.Status);
    Assert.Contains("Migration1", result.Description);
    Assert.Contains("Migration2", result.Description);
  }

  [Fact]
  public async Task ReturnsHealthy_WhenNoPendingMigrations()
  {
    // Arrange
    _migrationChecker.CanConnectAsync(Arg.Any<CancellationToken>()).Returns(true);
    _migrationChecker.GetPendingMigrationsAsync(Arg.Any<CancellationToken>())
      .Returns(Enumerable.Empty<string>());
    var healthCheck = new DatabaseMigrationHealthCheck(_migrationChecker, _cache);

    // Act
    var result = await healthCheck.CheckHealthAsync(null!, CancellationToken.None);

    // Assert
    Assert.Equal(HealthStatus.Healthy, result.Status);
    Assert.Equal("Database schema is up to date.", result.Description);
  }

  [Fact]
  public async Task CachesMigrationStatus_AfterFirstSuccessfulCheck()
  {
    // Arrange
    _migrationChecker.CanConnectAsync(Arg.Any<CancellationToken>()).Returns(true);
    _migrationChecker.GetPendingMigrationsAsync(Arg.Any<CancellationToken>())
      .Returns(Enumerable.Empty<string>());
    var healthCheck = new DatabaseMigrationHealthCheck(_migrationChecker, _cache);

    // Act - first check
    await healthCheck.CheckHealthAsync(null!, CancellationToken.None);

    // Act - second check
    await healthCheck.CheckHealthAsync(null!, CancellationToken.None);

    // Assert - GetPendingMigrationsAsync should only be called once due to caching
    await _migrationChecker.Received(1).GetPendingMigrationsAsync(Arg.Any<CancellationToken>());
  }
}
