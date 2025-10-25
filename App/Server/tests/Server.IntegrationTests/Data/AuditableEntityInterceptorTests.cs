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
