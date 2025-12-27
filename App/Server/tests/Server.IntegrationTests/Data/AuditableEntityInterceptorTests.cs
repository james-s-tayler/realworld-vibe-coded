using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Server.Core.ArticleAggregate;
using Server.Core.IdentityAggregate;
using Server.Core.TagAggregate;
using Server.Infrastructure;
using Server.Infrastructure.Data;
using Server.Infrastructure.Data.Interceptors;
using Server.SharedKernel.Interfaces;

namespace Server.IntegrationTests.Data;

/// <summary>
/// EF Core-specific integration tests for audit column functionality.
/// These tests verify that the AuditableEntityInterceptor correctly sets timestamps
/// on entities that inherit from EntityBase (Article, Comment, Tag).
/// Note: ApplicationUser does not inherit from EntityBase, so it's not tested here.
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

    services.AddDbContext<AppDbContext>(
      (serviceProvider, options) =>
      {
        options.UseInMemoryDatabase($"AuditTest_{Guid.NewGuid()}");
        options.AddInterceptors(serviceProvider.GetRequiredService<AuditableEntityInterceptor>());
      },
      ServiceLifetime.Transient);

    _serviceProvider = services.BuildServiceProvider();

    // Create the context manually with null dispatcher
    var dbContextOptions = _serviceProvider.GetRequiredService<DbContextOptions<AppDbContext>>();
    var multiTenantContextAccessor = new DefaultTenantContextAccessor();

    _dbContext = new AppDbContext(multiTenantContextAccessor, dbContextOptions, null);
  }

  [Fact]
  public async Task Interceptor_SetsCreatedAtAndUpdatedAt_OnEntityCreation()
  {
    // Arrange
    var fixedTime = DateTime.Parse("2025-10-25 12:00:00").ToUniversalTime();
    _timeProvider.SetTime(fixedTime);

    var author = CreateTestUser("author@example.com", "author");
    _dbContext.Users.Add(author);
    await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    var article = new Article(
      title: "Test Article",
      description: "Test Description",
      body: "Test Body",
      author: author
    );

    // Act
    _dbContext.Articles.Add(article);
    await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    // Assert
    article.CreatedAt.ShouldBe(fixedTime);
    article.UpdatedAt.ShouldBe(fixedTime);
  }

  [Fact]
  public async Task Interceptor_UpdatesOnlyUpdatedAt_OnEntityModification()
  {
    // Arrange
    var createTime = DateTime.Parse("2025-10-25 12:00:00").ToUniversalTime();
    _timeProvider.SetTime(createTime);

    var author = CreateTestUser("author@example.com", "author");
    _dbContext.Users.Add(author);
    await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    var article = new Article(
      title: "Test Article",
      description: "Test Description",
      body: "Test Body",
      author: author
    );

    _dbContext.Articles.Add(article);
    await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    var originalCreatedAt = article.CreatedAt;

    // Act - modify entity with new time
    var updateTime = DateTime.Parse("2025-10-25 13:00:00").ToUniversalTime();
    _timeProvider.SetTime(updateTime);

    article.Update("Test Article", "Test Description", "Updated Body");
    await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    // Assert
    article.CreatedAt.ShouldBe(originalCreatedAt); // CreatedAt should not change
    article.UpdatedAt.ShouldBe(updateTime); // UpdatedAt should be updated
    (article.UpdatedAt > article.CreatedAt).ShouldBeTrue();
  }

  [Fact]
  public async Task Interceptor_DoesNotUpdateUpdatedAt_WhenOnlyNavigationPropertiesChange()
  {
    // Arrange
    var createTime = DateTime.Parse("2025-10-25 12:00:00").ToUniversalTime();
    _timeProvider.SetTime(createTime);

    var author = CreateTestUser("author@example.com", "author");
    _dbContext.Users.Add(author);
    await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    var article = new Article(
      title: "Test Article",
      description: "Description",
      body: "Body",
      author: author
    );

    _dbContext.Articles.Add(article);
    await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    var originalUpdatedAt = article.UpdatedAt;

    // Act - modify only navigation property (add to favorites)
    var favoriteTime = DateTime.Parse("2025-10-25 13:00:00").ToUniversalTime();
    _timeProvider.SetTime(favoriteTime);

    var user2 = CreateTestUser("user2@example.com", "user2");
    _dbContext.Users.Add(user2);
    await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    article.AddToFavorites(user2);
    await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    // Assert
    article.UpdatedAt.ShouldBe(originalUpdatedAt); // UpdatedAt should NOT change for navigation-only changes
  }

  [Fact]
  public async Task Interceptor_HandlesMultipleEntities_InSingleSaveChanges()
  {
    // Arrange
    var fixedTime = DateTime.Parse("2025-10-25 12:00:00").ToUniversalTime();
    _timeProvider.SetTime(fixedTime);

    var author = CreateTestUser("author@example.com", "author");
    _dbContext.Users.Add(author);
    await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    var article1 = new Article("Article 1", "Description 1", "Body 1", author);
    var article2 = new Article("Article 2", "Description 2", "Body 2", author);
    var tag = new Tag("testtag");

    // Act
    _dbContext.Articles.AddRange(article1, article2);
    _dbContext.Tags.Add(tag);
    await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    // Assert
    article1.CreatedAt.ShouldBe(fixedTime);
    article1.UpdatedAt.ShouldBe(fixedTime);
    article2.CreatedAt.ShouldBe(fixedTime);
    article2.UpdatedAt.ShouldBe(fixedTime);
    tag.CreatedAt.ShouldBe(fixedTime);
    tag.UpdatedAt.ShouldBe(fixedTime);
  }

  [Fact]
  public async Task Interceptor_HandlesDifferentEntityTypes_InSameTransaction()
  {
    // Arrange
    var createTime = DateTime.Parse("2025-10-25 12:00:00").ToUniversalTime();
    _timeProvider.SetTime(createTime);

    var author = CreateTestUser("author@example.com", "author");
    _dbContext.Users.Add(author);
    await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    var article = new Article("Test Article", "Description", "Body", author);
    var comment = new Comment("Test comment", author, article);

    _dbContext.Articles.Add(article);
    _dbContext.Comments.Add(comment);
    await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    var originalArticleUpdatedAt = article.UpdatedAt;
    var originalCommentUpdatedAt = comment.UpdatedAt;

    // Act - update both entities
    var updateTime = DateTime.Parse("2025-10-25 13:00:00").ToUniversalTime();
    _timeProvider.SetTime(updateTime);

    article.Update("Test Article", "Description", "Updated Body");
    comment.Update("Updated comment");

    await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    // Assert
    article.UpdatedAt.ShouldBe(updateTime);
    article.CreatedAt.ShouldBe(createTime);
    comment.UpdatedAt.ShouldBe(updateTime);
    comment.CreatedAt.ShouldBe(createTime);
  }

  [Fact]
  public async Task Interceptor_SetsTimestamps_OnCommentEntity()
  {
    // Arrange
    var fixedTime = DateTime.Parse("2025-10-25 12:00:00").ToUniversalTime();
    _timeProvider.SetTime(fixedTime);

    var author = CreateTestUser("author@example.com", "author");
    _dbContext.Users.Add(author);
    await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    var article = new Article("Test Article", "Description", "Body", author);
    _dbContext.Articles.Add(article);
    await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    var comment = new Comment("Test comment body", author, article);

    // Act
    _dbContext.Comments.Add(comment);
    await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    // Assert
    comment.CreatedAt.ShouldBe(fixedTime);
    comment.UpdatedAt.ShouldBe(fixedTime);
  }

  [Fact]
  public async Task Interceptor_UpdatesComment_Timestamp()
  {
    // Arrange
    var createTime = DateTime.Parse("2025-10-25 12:00:00").ToUniversalTime();
    _timeProvider.SetTime(createTime);

    var author = CreateTestUser("author@example.com", "author");
    _dbContext.Users.Add(author);
    await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    var article = new Article("Test Article", "Description", "Body", author);
    _dbContext.Articles.Add(article);
    await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    var comment = new Comment("Original body", author, article);
    _dbContext.Comments.Add(comment);
    await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    var originalCreatedAt = comment.CreatedAt;

    // Act - update comment
    var updateTime = DateTime.Parse("2025-10-25 13:00:00").ToUniversalTime();
    _timeProvider.SetTime(updateTime);

    comment.Update("Updated body");
    await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    // Assert
    comment.CreatedAt.ShouldBe(originalCreatedAt);
    comment.UpdatedAt.ShouldBe(updateTime);
    (comment.UpdatedAt > comment.CreatedAt).ShouldBeTrue();
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

  private static ApplicationUser CreateTestUser(string email, string userName)
  {
    return new ApplicationUser
    {
      Id = Guid.NewGuid(),
      Email = email,
      UserName = userName,
      NormalizedEmail = email.ToUpperInvariant(),
      NormalizedUserName = userName.ToUpperInvariant(),
      Bio = "Test bio",
      Image = null,
    };
  }
}
