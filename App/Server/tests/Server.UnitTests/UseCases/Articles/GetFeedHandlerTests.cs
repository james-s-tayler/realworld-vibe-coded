using Server.Core.ArticleAggregate;
using Server.Core.AuthorAggregate;
using Server.Core.AuthorAggregate.Specifications;
using Server.SharedKernel.Persistence;
using Server.UseCases.Articles.Feed;

namespace Server.UnitTests.UseCases.Articles;

public class GetFeedHandlerTests
{
  [Fact]
  public async Task Handle_WhenAuthorNotFound_ReturnsEmptyResult()
  {
    var authorRepo = Substitute.For<IReadRepository<Author>>();
    var articleRepo = Substitute.For<IReadRepository<Article>>();

    authorRepo.FirstOrDefaultAsync(
      Arg.Any<AuthorWithFollowingByUserIdSpec>(),
      Arg.Any<CancellationToken>()).Returns((Author?)null);

    var handler = new GetFeedHandler(authorRepo, articleRepo);

    var result = await handler.Handle(new GetFeedQuery(Guid.NewGuid()), CancellationToken.None);

    result.IsSuccess.ShouldBeTrue();
    result.Value.Articles.ShouldBeEmpty();
    result.Value.TotalCount.ShouldBe(0);
  }
}
