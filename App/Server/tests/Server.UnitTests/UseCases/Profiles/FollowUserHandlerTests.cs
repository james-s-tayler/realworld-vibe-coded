using Server.Core.AuthorAggregate;
using Server.Core.AuthorAggregate.Specifications;
using Server.SharedKernel.Persistence;
using Server.SharedKernel.Result;
using Server.UseCases.Profiles.Follow;

namespace Server.UnitTests.UseCases.Profiles;

public class FollowUserHandlerTests
{
  private readonly IRepository<Author> _authorRepo = Substitute.For<IRepository<Author>>();

  [Fact]
  public async Task Handle_WhenAuthorToFollowNotFound_ReturnsNotFound()
  {
    _authorRepo.FirstOrDefaultAsync(
      Arg.Any<AuthorByUsernameSpec>(),
      Arg.Any<CancellationToken>()).Returns((Author?)null);

    var handler = new FollowUserHandler(_authorRepo);
    var result = await handler.Handle(new FollowUserCommand("unknown", Guid.NewGuid()), CancellationToken.None);

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

    var handler = new FollowUserHandler(_authorRepo);
    var result = await handler.Handle(new FollowUserCommand("target", Guid.NewGuid()), CancellationToken.None);

    result.Status.ShouldBe(ResultStatus.Error);
  }

  [Fact]
  public async Task Handle_WhenValid_FollowsAndReturnsSuccess()
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

    var handler = new FollowUserHandler(_authorRepo);
    var result = await handler.Handle(new FollowUserCommand("target", currentUserId), CancellationToken.None);

    result.IsSuccess.ShouldBeTrue();
    result.Value.ShouldBe(target);
    current.Following.Count.ShouldBe(1);
    await _authorRepo.Received(1).UpdateAsync(current, Arg.Any<CancellationToken>());
  }
}
