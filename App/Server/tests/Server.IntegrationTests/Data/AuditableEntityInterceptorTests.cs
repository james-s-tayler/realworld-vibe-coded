using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Server.Core.ArticleAggregate;
using Server.Core.UserAggregate;
using Server.Infrastructure.Data;
using Server.Infrastructure.Data.Interceptors;
using Server.SharedKernel.Interfaces;

namespace Server.IntegrationTests.Data;

/// <summary>
/// EF Core-specific integration tests for audit column functionality.
/// These tests verify that the AuditableEntityInterceptor correctly sets timestamps.
/// </summary>
public class AuditableEntityInterceptorTests : IDisposable
{
  private readonly ServiceProvider _serviceProvider;
  private readonly AppDbContext _dbContext;
  private readonly TestTimeProvider _timeProvider;

  public AuditableEntityInterceptorTests()
  {
    var services = new ServiceCollection();

    // Use controllable time provider for tests
    _timeProvider = new TestTimeProvider();
    services.AddSingleton<ITimeProvider>(_timeProvider);
    services.AddSingleton<AuditableEntityInterceptor>();

    services.AddDbContext<AppDbContext>((serviceProvider, options) =>
    {
      options.UseInMemoryDatabase($"AuditTest_{Guid.NewGuid()}");
      options.AddInterceptors(serviceProvider.GetRequiredService<AuditableEntityInterceptor>());
    }, ServiceLifetime.Transient);

    _serviceProvider = services.BuildServiceProvider();

    // Create the context manually with null dispatcher
    var dbContextOptions = _serviceProvider.GetRequiredService<DbContextOptions<AppDbContext>>();
    _dbContext = new AppDbContext(dbContextOptions, null);
  }

  [Fact]
  public async Task Interceptor_SetsCreatedAtAndUpdatedAt_OnEntityCreation()
  {
    // Arrange
    var fixedTime = new DateTime(2025, 10, 25, 12, 0, 0, DateTimeKind.Utc);
    _timeProvider.SetTime(fixedTime);

    var user = new User(
      email: "test@example.com",
      username: "testuser",
      hashedPassword: "hashedpass"
    );

    // Act
    _dbContext.Users.Add(user);
    await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    // Assert
    Assert.Equal(fixedTime, user.CreatedAt);
    Assert.Equal(fixedTime, user.UpdatedAt);
  }

  [Fact]
  public async Task Interceptor_UpdatesOnlyUpdatedAt_OnEntityModification()
  {
    // Arrange
    var createTime = new DateTime(2025, 10, 25, 12, 0, 0, DateTimeKind.Utc);
    _timeProvider.SetTime(createTime);

    var user = new User(
      email: "test@example.com",
      username: "testuser",
      hashedPassword: "hashedpass"
    );

    _dbContext.Users.Add(user);
    await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    var originalCreatedAt = user.CreatedAt;

    // Act - modify entity with new time
    var updateTime = new DateTime(2025, 10, 25, 13, 0, 0, DateTimeKind.Utc);
    _timeProvider.SetTime(updateTime);

    user.UpdateBio("New bio");
    await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    // Assert
    Assert.Equal(originalCreatedAt, user.CreatedAt); // CreatedAt should not change
    Assert.Equal(updateTime, user.UpdatedAt); // UpdatedAt should be updated
    Assert.True(user.UpdatedAt > user.CreatedAt);
  }

  [Fact]
  public async Task Interceptor_DoesNotUpdateUpdatedAt_WhenOnlyNavigationPropertiesChange()
  {
    // Arrange
    var createTime = new DateTime(2025, 10, 25, 12, 0, 0, DateTimeKind.Utc);
    _timeProvider.SetTime(createTime);

    var user = new User(
      email: "author@example.com",
      username: "author",
      hashedPassword: "hashedpass"
    );

    var article = new Article(
      title: "Test Article",
      description: "Description",
      body: "Body",
      author: user
    );

    _dbContext.Users.Add(user);
    _dbContext.Articles.Add(article);
    await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    var originalUpdatedAt = article.UpdatedAt;

    // Act - modify only navigation property (add to favorites)
    var favoriteTime = new DateTime(2025, 10, 25, 13, 0, 0, DateTimeKind.Utc);
    _timeProvider.SetTime(favoriteTime);

    var user2 = new User(
      email: "user2@example.com",
      username: "user2",
      hashedPassword: "hashedpass"
    );
    _dbContext.Users.Add(user2);
    await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    article.AddToFavorites(user2);
    await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    // Assert
    Assert.Equal(originalUpdatedAt, article.UpdatedAt); // UpdatedAt should NOT change for navigation-only changes
  }

  [Fact]
  public async Task Interceptor_HandlesMultipleEntities_InSingleSaveChanges()
  {
    // Arrange
    var fixedTime = new DateTime(2025, 10, 25, 12, 0, 0, DateTimeKind.Utc);
    _timeProvider.SetTime(fixedTime);

    var user1 = new User("user1@example.com", "user1", "pass1");
    var user2 = new User("user2@example.com", "user2", "pass2");
    var tag = new Tag("testtag");

    // Act
    _dbContext.Users.AddRange(user1, user2);
    _dbContext.Tags.Add(tag);
    await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    // Assert
    Assert.Equal(fixedTime, user1.CreatedAt);
    Assert.Equal(fixedTime, user1.UpdatedAt);
    Assert.Equal(fixedTime, user2.CreatedAt);
    Assert.Equal(fixedTime, user2.UpdatedAt);
    Assert.Equal(fixedTime, tag.CreatedAt);
    Assert.Equal(fixedTime, tag.UpdatedAt);
  }

  [Fact]
  public async Task Interceptor_OverridesManualCreatedAtSetting()
  {
    // Arrange
    var manualTime = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    var actualTime = new DateTime(2025, 10, 25, 12, 0, 0, DateTimeKind.Utc);
    _timeProvider.SetTime(actualTime);

    var user = new User("test@example.com", "testuser", "hashedpass");

    // Act - Try to manually set CreatedAt (should be overridden by interceptor)
    user.CreatedAt = manualTime;
    _dbContext.Users.Add(user);
    await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    // Assert - Framework should override manual value
    Assert.Equal(actualTime, user.CreatedAt);
    Assert.NotEqual(manualTime, user.CreatedAt);
  }

  [Fact]
  public async Task Interceptor_OverridesManualUpdatedAtSetting()
  {
    // Arrange
    var createTime = new DateTime(2025, 10, 25, 12, 0, 0, DateTimeKind.Utc);
    _timeProvider.SetTime(createTime);

    var user = new User("test@example.com", "testuser", "hashedpass");
    _dbContext.Users.Add(user);
    await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    // Act - Try to manually set UpdatedAt (should be overridden by interceptor)
    var manualTime = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    var actualUpdateTime = new DateTime(2025, 10, 25, 13, 0, 0, DateTimeKind.Utc);
    _timeProvider.SetTime(actualUpdateTime);

    user.UpdateBio("New bio");
    user.UpdatedAt = manualTime; // Try to override
    await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    // Assert - Framework should override manual value
    Assert.Equal(actualUpdateTime, user.UpdatedAt);
    Assert.NotEqual(manualTime, user.UpdatedAt);
  }

  [Fact]
  public async Task Interceptor_OverridesManualCreatedBySetting()
  {
    // Arrange
    var fixedTime = new DateTime(2025, 10, 25, 12, 0, 0, DateTimeKind.Utc);
    _timeProvider.SetTime(fixedTime);

    var user = new User("test@example.com", "testuser", "hashedpass");

    // Act - Try to manually set CreatedBy (should be overridden by interceptor)
    user.CreatedBy = "ManualUser";
    _dbContext.Users.Add(user);
    await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    // Assert - Framework should override with "SYSTEM" (no authenticated user in test)
    Assert.Equal("SYSTEM", user.CreatedBy);
    Assert.NotEqual("ManualUser", user.CreatedBy);
  }

  [Fact]
  public async Task Interceptor_OverridesManualUpdatedBySetting()
  {
    // Arrange
    var createTime = new DateTime(2025, 10, 25, 12, 0, 0, DateTimeKind.Utc);
    _timeProvider.SetTime(createTime);

    var user = new User("test@example.com", "testuser", "hashedpass");
    _dbContext.Users.Add(user);
    await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    // Act - Try to manually set UpdatedBy (should be overridden by interceptor)
    var updateTime = new DateTime(2025, 10, 25, 13, 0, 0, DateTimeKind.Utc);
    _timeProvider.SetTime(updateTime);

    user.UpdateBio("New bio");
    user.UpdatedBy = "ManualUser"; // Try to override
    await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    // Assert - Framework should override with "SYSTEM" (no authenticated user in test)
    Assert.Equal("SYSTEM", user.UpdatedBy);
    Assert.NotEqual("ManualUser", user.UpdatedBy);
  }

  [Fact]
  public async Task Interceptor_OverridesAllManualAuditFieldSettings()
  {
    // Arrange
    var manualTime = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    var actualTime = new DateTime(2025, 10, 25, 12, 0, 0, DateTimeKind.Utc);
    _timeProvider.SetTime(actualTime);

    var user = new User("test@example.com", "testuser", "hashedpass");

    // Act - Try to manually set ALL audit fields (should all be overridden)
    user.CreatedAt = manualTime;
    user.UpdatedAt = manualTime;
    user.CreatedBy = "ManualCreator";
    user.UpdatedBy = "ManualUpdater";

    _dbContext.Users.Add(user);
    await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    // Assert - Framework should override all manual values
    Assert.Equal(actualTime, user.CreatedAt);
    Assert.Equal(actualTime, user.UpdatedAt);
    Assert.Equal("SYSTEM", user.CreatedBy);
    Assert.Equal("SYSTEM", user.UpdatedBy);
  }

  [Fact]
  public void Interceptor_SetsCreatedAtAndUpdatedAt_OnEntityCreation_Synchronous()
  {
    // Arrange
    var fixedTime = new DateTime(2025, 10, 25, 12, 0, 0, DateTimeKind.Utc);
    _timeProvider.SetTime(fixedTime);

    var user = new User("sync@example.com", "syncuser", "hashedpass");

    // Act
    _dbContext.Users.Add(user);
    _dbContext.SaveChanges(); // Synchronous save

    // Assert
    Assert.Equal(fixedTime, user.CreatedAt);
    Assert.Equal(fixedTime, user.UpdatedAt);
    Assert.Equal("SYSTEM", user.CreatedBy);
    Assert.Equal("SYSTEM", user.UpdatedBy);
  }

  [Fact]
  public void Interceptor_UpdatesOnlyUpdatedAtAndUpdatedBy_OnEntityModification_Synchronous()
  {
    // Arrange
    var creationTime = new DateTime(2025, 10, 25, 12, 0, 0, DateTimeKind.Utc);
    _timeProvider.SetTime(creationTime);

    var user = new User("sync2@example.com", "syncuser2", "hashedpass");

    _dbContext.Users.Add(user);
    _dbContext.SaveChanges(); // Synchronous save

    var originalCreatedAt = user.CreatedAt;
    var originalCreatedBy = user.CreatedBy;

    // Change time for the update
    var updateTime = creationTime.AddHours(2);
    _timeProvider.SetTime(updateTime);

    // Act - Modify the entity
    user.UpdateBio("Updated bio sync");
    _dbContext.SaveChanges(); // Synchronous save

    // Assert
    Assert.Equal(originalCreatedAt, user.CreatedAt); // Should not change
    Assert.Equal(originalCreatedBy, user.CreatedBy); // Should not change
    Assert.Equal(updateTime, user.UpdatedAt); // Should be updated
    Assert.Equal("SYSTEM", user.UpdatedBy); // Should be updated
  }

  [Fact]
  public void Interceptor_DoesNotUpdateAuditFields_WhenOnlyNavigationPropertiesChange_Synchronous()
  {
    // Arrange
    var creationTime = new DateTime(2025, 10, 25, 12, 0, 0, DateTimeKind.Utc);
    _timeProvider.SetTime(creationTime);

    var author = new User("author@example.com", "author", "hashedpass");

    var article = new Article(
      title: "Sync Test Article",
      description: "Sync Test Description",
      body: "Sync Test Body",
      author);

    _dbContext.Users.Add(author);
    _dbContext.Articles.Add(article);
    _dbContext.SaveChanges(); // Synchronous save

    var originalUpdatedAt = article.UpdatedAt;

    // Change time for the "update"
    var updateTime = creationTime.AddHours(2);
    _timeProvider.SetTime(updateTime);

    // Act - Touch the article entity without changing any scalar properties
    var entry = _dbContext.Entry(article);
    entry.State = EntityState.Modified;

    _dbContext.SaveChanges(); // Synchronous save

    // Assert
    // Since no actual property values changed, UpdatedAt should NOT be updated
    Assert.Equal(originalUpdatedAt, article.UpdatedAt);
  }

  public void Dispose()
  {
    _dbContext?.Dispose();
    _serviceProvider?.Dispose();
  }

  /// <summary>
  /// Test time provider with controllable time.
  /// </summary>
  private class TestTimeProvider : ITimeProvider
  {
    private DateTime _currentTime = DateTime.UtcNow;

    public DateTime UtcNow => _currentTime;

    public void SetTime(DateTime time)
    {
      _currentTime = time;
    }
  }
}
