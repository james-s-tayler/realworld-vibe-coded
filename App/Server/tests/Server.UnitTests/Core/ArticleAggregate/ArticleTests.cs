using Server.Core.ArticleAggregate;
using Server.Core.AuthorAggregate;

namespace Server.UnitTests.Core.ArticleAggregate;

public class ArticleTests
{
  [Fact]
  public void AddToFavorites_AddsAuthor()
  {
    var author = CreateAuthor();
    var article = new Article("Title", "desc", "body", author);
    var reader = new Author(Guid.NewGuid(), "reader", "bio", null);

    article.AddToFavorites(reader);

    article.FavoritedBy.ShouldContain(reader);
    article.FavoritesCount.ShouldBe(1);
  }

  [Fact]
  public void AddToFavorites_AlreadyFavorited_DoesNotDuplicate()
  {
    var author = CreateAuthor();
    var article = new Article("Title", "desc", "body", author);
    var reader = new Author(Guid.NewGuid(), "reader", "bio", null);

    article.AddToFavorites(reader);
    article.AddToFavorites(reader);

    article.FavoritesCount.ShouldBe(1);
  }

  [Fact]
  public void RemoveFromFavorites_RemovesAuthor()
  {
    var author = CreateAuthor();
    var article = new Article("Title", "desc", "body", author);
    var reader = new Author(Guid.NewGuid(), "reader", "bio", null);

    article.AddToFavorites(reader);
    article.RemoveFromFavorites(reader);

    article.FavoritedBy.ShouldNotContain(reader);
    article.FavoritesCount.ShouldBe(0);
  }

  [Fact]
  public void IsFavoritedBy_WhenFavorited_ReturnsTrue()
  {
    var author = CreateAuthor();
    var article = new Article("Title", "desc", "body", author);
    var reader = new Author(Guid.NewGuid(), "reader", "bio", null);

    article.AddToFavorites(reader);

    article.IsFavoritedBy(reader.Id).ShouldBeTrue();
  }

  [Fact]
  public void IsFavoritedBy_WhenNotFavorited_ReturnsFalse()
  {
    var author = CreateAuthor();
    var article = new Article("Title", "desc", "body", author);

    article.IsFavoritedBy(Guid.NewGuid()).ShouldBeFalse();
  }

  [Fact]
  public void IsFavoritedBy_NullUserId_ReturnsFalse()
  {
    var author = CreateAuthor();
    var article = new Article("Title", "desc", "body", author);

    article.IsFavoritedBy(null).ShouldBeFalse();
  }

  [Fact]
  public void Constructor_SetsSlugFromTitle()
  {
    var author = CreateAuthor();
    var article = new Article("My Great Article!", "desc", "body", author);

    article.Slug.ShouldBe("my-great-article");
  }

  [Fact]
  public void Update_UpdatesTitleAndSlug()
  {
    var author = CreateAuthor();
    var article = new Article("Old Title", "old desc", "old body", author);

    article.Update("New Title", "new desc", "new body");

    article.Title.ShouldBe("New Title");
    article.Slug.ShouldBe("new-title");
    article.Description.ShouldBe("new desc");
    article.Body.ShouldBe("new body");
  }

  [Fact]
  public void AddTag_AddsTag()
  {
    var author = CreateAuthor();
    var article = new Article("Title", "desc", "body", author);
    var tag = new Server.Core.TagAggregate.Tag("test-tag");

    article.AddTag(tag);

    article.Tags.ShouldContain(tag);
  }

  [Fact]
  public void AddTag_Duplicate_DoesNotAdd()
  {
    var author = CreateAuthor();
    var article = new Article("Title", "desc", "body", author);
    var tag = new Server.Core.TagAggregate.Tag("test-tag");

    article.AddTag(tag);
    article.AddTag(tag);

    article.Tags.Count.ShouldBe(1);
  }

  [Fact]
  public void RemoveTag_RemovesTag()
  {
    var author = CreateAuthor();
    var article = new Article("Title", "desc", "body", author);
    var tag = new Server.Core.TagAggregate.Tag("test-tag");

    article.AddTag(tag);
    article.RemoveTag(tag);

    article.Tags.ShouldBeEmpty();
  }

  private Author CreateAuthor() => new(Guid.NewGuid(), "testuser", "bio", null);
}
