using Server.Core.AuthorAggregate;
using Server.Core.AuthorAggregate.Specifications;
using Server.SharedKernel.Persistence;
using Server.SharedKernel.Result;
using Server.UseCases.Profiles.Get;

namespace Server.UnitTests.UseCases.Profiles;

public class GetProfileHandlerTests
{
  private readonly IRepository<Author> _authorRepo = Substitute.For<IRepository<Author>>();

  [Fact]
  public async Task Handle_WhenAuthorNotFound_ReturnsNotFound()
  {
    _authorRepo.FirstOrDefaultAsync(
      Arg.Any<AuthorWithRelationshipsByUsernameSpec>(),
      Arg.Any<CancellationToken>()).Returns((Author?)null);

    var handler = new GetProfileHandler(_authorRepo);
    var result = await handler.Handle(new GetProfileQuery("unknown"), CancellationToken.None);

    result.Status.ShouldBe(ResultStatus.NotFound);
  }

  [Fact]
  public async Task Handle_WhenAuthorFound_ReturnsSuccess()
  {
    var author = new Author(Guid.NewGuid(), "testuser", "bio", null);
    _authorRepo.FirstOrDefaultAsync(
      Arg.Any<AuthorWithRelationshipsByUsernameSpec>(),
      Arg.Any<CancellationToken>()).Returns(author);

    var handler = new GetProfileHandler(_authorRepo);
    var result = await handler.Handle(new GetProfileQuery("testuser"), CancellationToken.None);

    result.IsSuccess.ShouldBeTrue();
    result.Value.Username.ShouldBe("testuser");
  }
}
