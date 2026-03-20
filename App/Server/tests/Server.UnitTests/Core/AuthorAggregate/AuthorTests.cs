using Server.Core.AuthorAggregate;

namespace Server.UnitTests.Core.AuthorAggregate;

public class AuthorTests
{
  [Fact]
  public void Constructor_WithNullBio_DefaultsToEmptyString()
  {
    var author = new Author(Guid.NewGuid(), "testuser", null!, null);

    author.Bio.ShouldBe(string.Empty);
  }

  [Fact]
  public void Update_WithNullBio_DefaultsToEmptyString()
  {
    var author = new Author(Guid.NewGuid(), "testuser", "some bio", null);

    author.Update("testuser", null!, null);

    author.Bio.ShouldBe(string.Empty);
  }

  [Fact]
  public void Follow_Self_DoesNothing()
  {
    var authorId = Guid.NewGuid();
    var author = new Author(authorId, "testuser", "bio", null);

    author.Follow(author);

    author.Following.ShouldBeEmpty();
  }

  [Fact]
  public void Follow_AnotherAuthor_AddsToFollowing()
  {
    var author = new Author(Guid.NewGuid(), "user1", "bio", null);
    var other = new Author(Guid.NewGuid(), "user2", "bio", null);

    author.Follow(other);

    author.Following.Count.ShouldBe(1);
    author.IsFollowing(other).ShouldBeTrue();
  }

  [Fact]
  public void Follow_AlreadyFollowing_DoesNotDuplicate()
  {
    var author = new Author(Guid.NewGuid(), "user1", "bio", null);
    var other = new Author(Guid.NewGuid(), "user2", "bio", null);

    author.Follow(other);
    author.Follow(other);

    author.Following.Count.ShouldBe(1);
  }

  [Fact]
  public void Unfollow_NotFollowing_DoesNothing()
  {
    var author = new Author(Guid.NewGuid(), "user1", "bio", null);
    var other = new Author(Guid.NewGuid(), "user2", "bio", null);

    author.Unfollow(other);

    author.Following.ShouldBeEmpty();
  }

  [Fact]
  public void Unfollow_Following_RemovesFromFollowing()
  {
    var author = new Author(Guid.NewGuid(), "user1", "bio", null);
    var other = new Author(Guid.NewGuid(), "user2", "bio", null);

    author.Follow(other);
    author.Unfollow(other);

    author.Following.ShouldBeEmpty();
    author.IsFollowing(other).ShouldBeFalse();
  }
}
