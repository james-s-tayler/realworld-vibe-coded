using Server.Core.ArticleAggregate;

namespace Server.UnitTests.UseCases.Articles;

/// <summary>
/// Unit tests for Article slug generation
/// </summary>
public class ArticleSlugTests
{
  [Fact]
  public void GenerateSlug_Should_Create_Url_Friendly_Slug()
  {
    // Arrange
    var title = "Hello World! This is a Test.";

    // Act
    var slug = Article.GenerateSlug(title);

    // Assert
    Assert.Equal("hello-world-this-is-a-test", slug);
  }

  [Fact]
  public void GenerateSlug_Should_Handle_Special_Characters()
  {
    // Arrange
    var title = "What's up? Let's test, \"quotes\" and more!";

    // Act
    var slug = Article.GenerateSlug(title);

    // Assert
    Assert.Equal("whats-up-lets-test-quotes-and-more", slug);
  }
}
