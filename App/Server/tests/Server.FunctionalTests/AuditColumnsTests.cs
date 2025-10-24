using Server.Core.ArticleAggregate;
using Server.Core.UserAggregate;
using Server.FunctionalTests.Articles.Fixture;
using Server.Infrastructure.Data;

namespace Server.FunctionalTests;

[Collection("Articles Integration Tests")]
public class AuditColumnsTests(ArticlesFixture App) : TestBase<ArticlesFixture>
{
  [Fact]
  public async Task EntityBase_CreatedAt_IsSetAutomaticallyOnCreation()
  {
    // Arrange
    var scope = App.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var beforeCreate = DateTime.UtcNow.AddSeconds(-1);

    // Act
    var user = new User(
      email: $"audit-test-{Guid.NewGuid()}@example.com",
      username: $"audituser-{Guid.NewGuid()}",
      hashedPassword: "hashedpass123"
    );

    dbContext.Users.Add(user);
    await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    var afterCreate = DateTime.UtcNow.AddSeconds(1);

    // Assert
    user.CreatedAt.ShouldBeGreaterThanOrEqualTo(beforeCreate);
    user.CreatedAt.ShouldBeLessThanOrEqualTo(afterCreate);
    // On creation, UpdatedAt should be within a second of CreatedAt
    (user.UpdatedAt - user.CreatedAt).TotalSeconds.ShouldBeLessThan(1);
  }

  [Fact]
  public async Task EntityBase_UpdatedAt_IsSetAutomaticallyOnModification()
  {
    // Arrange
    var scope = App.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var user = new User(
      email: $"update-test-{Guid.NewGuid()}@example.com",
      username: $"updateuser-{Guid.NewGuid()}",
      hashedPassword: "hashedpass123"
    );

    dbContext.Users.Add(user);
    await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    var originalCreatedAt = user.CreatedAt;
    var originalUpdatedAt = user.UpdatedAt;

    await Task.Delay(100, TestContext.Current.CancellationToken); // Ensure enough time passes for timestamp difference
    var beforeUpdate = DateTime.UtcNow;
    await Task.Delay(10, TestContext.Current.CancellationToken);

    // Act
    user.UpdateBio("Updated bio");
    await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    await Task.Delay(10, TestContext.Current.CancellationToken);
    var afterUpdate = DateTime.UtcNow;

    // Assert
    user.CreatedAt.ShouldBe(originalCreatedAt); // CreatedAt should not change
    user.UpdatedAt.ShouldBeGreaterThan(originalUpdatedAt); // UpdatedAt should be updated
    user.UpdatedAt.ShouldBeGreaterThanOrEqualTo(beforeUpdate);
    user.UpdatedAt.ShouldBeLessThanOrEqualTo(afterUpdate);
  }

  [Fact]
  public async Task Article_AuditColumns_AreSetAutomatically()
  {
    // Arrange
    var scope = App.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var uniqueId = Guid.NewGuid();
    var user = new User(
      email: $"article-audit-{uniqueId}@example.com",
      username: $"articleuser-{uniqueId}",
      hashedPassword: "hashedpass123"
    );

    dbContext.Users.Add(user);
    await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    var beforeCreate = DateTime.UtcNow.AddSeconds(-1);

    // Act - Create article
    var article = new Article(
      title: $"Audit Test Article {uniqueId}",
      description: "Test Description",
      body: "Test Body",
      author: user
    );

    dbContext.Articles.Add(article);
    await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    var afterCreate = DateTime.UtcNow.AddSeconds(1);

    // Assert - Creation
    article.CreatedAt.ShouldBeGreaterThanOrEqualTo(beforeCreate);
    article.CreatedAt.ShouldBeLessThanOrEqualTo(afterCreate);
    (article.UpdatedAt - article.CreatedAt).TotalSeconds.ShouldBeLessThan(1);

    var originalCreatedAt = article.CreatedAt;
    await Task.Delay(100, TestContext.Current.CancellationToken);
    var beforeUpdate = DateTime.UtcNow.AddSeconds(-1);

    // Act - Update article
    article.Update($"Audit Updated Article {uniqueId}", "Updated Description", "Updated Body");
    await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    var afterUpdate = DateTime.UtcNow.AddSeconds(1);

    // Assert - Update
    article.CreatedAt.ShouldBe(originalCreatedAt); // CreatedAt should not change
    article.UpdatedAt.ShouldBeGreaterThan(originalCreatedAt);
    article.UpdatedAt.ShouldBeGreaterThanOrEqualTo(beforeUpdate);
    article.UpdatedAt.ShouldBeLessThanOrEqualTo(afterUpdate);
  }

  [Fact]
  public async Task Comment_AuditColumns_AreSetAutomatically()
  {
    // Arrange
    var scope = App.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var user = new User(
      email: $"comment-audit-{Guid.NewGuid()}@example.com",
      username: $"commentuser-{Guid.NewGuid()}",
      hashedPassword: "hashedpass123"
    );

    var article = new Article(
      title: $"Article for Comment {Guid.NewGuid()}",
      description: "Test Description",
      body: "Test Body",
      author: user
    );

    dbContext.Users.Add(user);
    dbContext.Articles.Add(article);
    await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    var beforeCreate = DateTime.UtcNow.AddSeconds(-1);

    // Act - Create comment
    var comment = new Comment("Test comment body", user, article);

    dbContext.Comments.Add(comment);
    await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    var afterCreate = DateTime.UtcNow.AddSeconds(1);

    // Assert
    comment.CreatedAt.ShouldBeGreaterThanOrEqualTo(beforeCreate);
    comment.CreatedAt.ShouldBeLessThanOrEqualTo(afterCreate);
    (comment.UpdatedAt - comment.CreatedAt).TotalSeconds.ShouldBeLessThan(1);
  }

  [Fact]
  public async Task Tag_AuditColumns_AreSetAutomatically()
  {
    // Arrange
    var scope = App.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var beforeCreate = DateTime.UtcNow.AddSeconds(-1);

    // Act
    var tag = new Tag($"testtag-{Guid.NewGuid()}");

    dbContext.Tags.Add(tag);
    await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    var afterCreate = DateTime.UtcNow.AddSeconds(1);

    // Assert
    tag.CreatedAt.ShouldBeGreaterThanOrEqualTo(beforeCreate);
    tag.CreatedAt.ShouldBeLessThanOrEqualTo(afterCreate);
    (tag.UpdatedAt - tag.CreatedAt).TotalSeconds.ShouldBeLessThan(1);
  }
}
