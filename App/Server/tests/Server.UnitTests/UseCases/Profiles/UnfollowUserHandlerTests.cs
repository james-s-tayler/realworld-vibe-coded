using Server.Core.AuthorAggregate;
using Server.Core.AuthorAggregate.Specifications;
using Server.SharedKernel.Persistence;
using Server.SharedKernel.Result;
using Server.UseCases.Profiles.Unfollow;

namespace Server.UnitTests.UseCases.Profiles;

public class UnfollowUserHandlerTests
{
  private readonly IRepository<Author> _authorRepo = Substitute.For<IRepository<Author>>();

  [Fact]
  public async Task Handle_WhenAuthorToUnfollowNotFound_ReturnsNotFound()
  {
    _authorRepo.FirstOrDefaultAsync(
      Arg.Any<AuthorByUsernameSpec>(),
      Arg.Any<CancellationToken>()).Returns((Author?)null);

    var handler = new UnfollowUserHandler(_authorRepo);
    var result = await handler.Handle(new UnfollowUserCommand("unknown", Guid.NewGuid()), CancellationToken.None);

    result.Status.ShouldBe(ResultStatus.NotFound);
  }

  [Fact]
  public async Task Handle_WhenCurrentAuthorNotFound_ReturnsError()
  {
    var target = new Author(Guid.NewGuid(), "target", "bio", null);
    _authorRepo.FirstOrDefaultAsync(
      Arg.Any<AuthorByUsernameSpec>(),
      Arg.Any<CancellationToken>()).Returns(target);
    _authorRepo.FirstOrDefaultAsync(
      Arg.Any<AuthorWithFollowingByUserIdSpec>(),
      Arg.Any<CancellationToken>()).Returns((Author?)null);

    var handler = new UnfollowUserHandler(_authorRepo);
    var result = await handler.Handle(new UnfollowUserCommand("target", Guid.NewGuid()), CancellationToken.None);

    result.Status.ShouldBe(ResultStatus.Error);
  }

  [Fact]
  public async Task Handle_WhenNotFollowing_ReturnsInvalid()
  {
    var targetId = Guid.NewGuid();
    var currentUserId = Guid.NewGuid();
    var target = new Author(targetId, "target", "bio", null);
    var current = new Author(currentUserId, "current", "bio", null);

    _authorRepo.FirstOrDefaultAsync(
      Arg.Any<AuthorByUsernameSpec>(),
      Arg.Any<CancellationToken>()).Returns(target);
    _authorRepo.FirstOrDefaultAsync(
      Arg.Any<AuthorWithFollowingByUserIdSpec>(),
      Arg.Any<CancellationToken>()).Returns(current);

    var handler = new UnfollowUserHandler(_authorRepo);
    var result = await handler.Handle(new UnfollowUserCommand("target", currentUserId), CancellationToken.None);

    result.Status.ShouldBe(ResultStatus.Invalid);
  }

  [Fact]
  public async Task Handle_WhenFollowing_UnfollowsAndReturnsSuccess()
  {
    var targetId = Guid.NewGuid();
    var currentUserId = Guid.NewGuid();
    var target = new Author(targetId, "target", "bio", null);
    var current = new Author(currentUserId, "current", "bio", null);
    current.Follow(target);

    _authorRepo.FirstOrDefaultAsync(
      Arg.Any<AuthorByUsernameSpec>(),
      Arg.Any<CancellationToken>()).Returns(target);
    _authorRepo.FirstOrDefaultAsync(
      Arg.Any<AuthorWithFollowingByUserIdSpec>(),
      Arg.Any<CancellationToken>()).Returns(current);

    var handler = new UnfollowUserHandler(_authorRepo);
    var result = await handler.Handle(new UnfollowUserCommand("target", currentUserId), CancellationToken.None);

    result.IsSuccess.ShouldBeTrue();
    result.Value.ShouldBe(target);
    current.Following.ShouldBeEmpty();
    await _authorRepo.Received(1).UpdateAsync(current, Arg.Any<CancellationToken>());
  }
}
