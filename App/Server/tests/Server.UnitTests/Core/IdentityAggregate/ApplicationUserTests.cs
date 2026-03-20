using Server.Core.IdentityAggregate;

namespace Server.UnitTests.Core.IdentityAggregate;

public class ApplicationUserTests
{
  [Fact]
  public void Constructor_SetsDefaultBioAndImage()
  {
    var user = new ApplicationUser();

    user.Bio.ShouldBe("I work at statefarm");
    user.Image.ShouldBeNull();
    user.Following.ShouldBeEmpty();
    user.Followers.ShouldBeEmpty();
  }

  [Fact]
  public void Follow_Self_DoesNothing()
  {
    var user = new ApplicationUser { Id = Guid.NewGuid() };

    user.Follow(user);

    user.Following.ShouldBeEmpty();
  }

  [Fact]
  public void Follow_AnotherUser_AddsToFollowing()
  {
    var user = new ApplicationUser { Id = Guid.NewGuid() };
    var other = new ApplicationUser { Id = Guid.NewGuid() };

    user.Follow(other);

    user.Following.Count.ShouldBe(1);
    user.IsFollowing(other).ShouldBeTrue();
  }

  [Fact]
  public void Follow_AlreadyFollowing_DoesNotDuplicate()
  {
    var user = new ApplicationUser { Id = Guid.NewGuid() };
    var other = new ApplicationUser { Id = Guid.NewGuid() };

    user.Follow(other);
    user.Follow(other);

    user.Following.Count.ShouldBe(1);
  }

  [Fact]
  public void Unfollow_Following_RemovesFromFollowing()
  {
    var user = new ApplicationUser { Id = Guid.NewGuid() };
    var other = new ApplicationUser { Id = Guid.NewGuid() };

    user.Follow(other);
    user.Unfollow(other);

    user.Following.ShouldBeEmpty();
    user.IsFollowing(other).ShouldBeFalse();
  }

  [Fact]
  public void Unfollow_NotFollowing_DoesNothing()
  {
    var user = new ApplicationUser { Id = Guid.NewGuid() };
    var other = new ApplicationUser { Id = Guid.NewGuid() };

    user.Unfollow(other);

    user.Following.ShouldBeEmpty();
  }

  [Fact]
  public void IsFollowing_ById_ReturnsTrueWhenFollowing()
  {
    var user = new ApplicationUser { Id = Guid.NewGuid() };
    var other = new ApplicationUser { Id = Guid.NewGuid() };

    user.Follow(other);

    user.IsFollowing(other.Id).ShouldBeTrue();
  }

  [Fact]
  public void IsFollowing_ById_ReturnsFalseWhenNotFollowing()
  {
    var user = new ApplicationUser { Id = Guid.NewGuid() };

    user.IsFollowing(Guid.NewGuid()).ShouldBeFalse();
  }
}
