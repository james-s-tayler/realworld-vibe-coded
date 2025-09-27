using Server.Core.ArticleAggregate;
using Server.Core.UserAggregate;
using Server.UseCases.Articles;

namespace Server.UnitTests.UseCases.Articles;

/// <summary>
/// Unit tests for ArticleMappers to ensure proper mapping behavior
/// </summary>
public class ArticleMappersTests
{
  [Fact]
  public void MapToDto_Should_Map_Article_To_ArticleDto_Without_CurrentUser()
  {
    // Arrange
    var author = new User("author@test.com", "testauthor", "hashedpassword");
    var article = new Article("Test Title", "Test Description", "Test Body", author);

    // Act
    var result = ArticleMappers.MapToDto(article);

    // Assert
    Assert.Equal(article.Slug, result.Slug);
    Assert.Equal(article.Title, result.Title);
    Assert.Equal(article.Description, result.Description);
    Assert.Equal(article.Body, result.Body);
    Assert.Equal(article.CreatedAt, result.CreatedAt);
    Assert.Equal(article.UpdatedAt, result.UpdatedAt);
    Assert.False(result.Favorited); // No current user, so should be false
    Assert.Equal(0, result.FavoritesCount);
    Assert.Equal(author.Username, result.Author.Username);
    Assert.False(result.Author.Following); // No current user, so should be false
  }

  [Fact]
  public void GenerateSlug_Should_Create_Url_Friendly_Slug()
  {
    // Arrange
    var title = "Hello World! This is a Test.";

    // Act
    var slug = ArticleMappers.GenerateSlug(title);

    // Assert
    Assert.Equal("hello-world-this-is-a-test", slug);
  }

  [Fact]
  public void GenerateSlug_Should_Handle_Special_Characters()
  {
    // Arrange
    var title = "What's up? Let's test, \"quotes\" and more!";

    // Act
    var slug = ArticleMappers.GenerateSlug(title);

    // Assert
    Assert.Equal("whats-up-lets-test-quotes-and-more", slug);
  }

  [Fact]
  public void MapToDto_With_Explicit_Favorited_Should_Use_Provided_Value()
  {
    // Arrange
    var author = new User("author@test.com", "testauthor", "hashedpassword");
    var article = new Article("Test Title", "Test Description", "Test Body", author);

    // Act
    var result = ArticleMappers.MapToDto(article, null, true);

    // Assert
    Assert.True(result.Favorited);
  }
}