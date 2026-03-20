using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Specifications.Articles;
using Server.Core.AuthorAggregate;
using Server.Core.AuthorAggregate.Specifications;
using Server.SharedKernel.Persistence;
using Server.SharedKernel.Result;
using Server.UseCases.Articles.Comments.Get;

namespace Server.UnitTests.UseCases.Articles;

public class GetCommentsHandlerTests
{
  private readonly IRepository<Article> _articleRepo = Substitute.For<IRepository<Article>>();
  private readonly IReadRepository<Author> _authorRepo = Substitute.For<IReadRepository<Author>>();

  [Fact]
  public async Task Handle_WhenArticleNotFound_ReturnsNotFound()
  {
    _articleRepo.FirstOrDefaultAsync(
      Arg.Any<ArticleBySlugSpec>(),
      Arg.Any<CancellationToken>()).Returns((Article?)null);

    var handler = new GetCommentsHandler(_articleRepo, _authorRepo);
    var result = await handler.Handle(new GetCommentsQuery("missing-slug"), CancellationToken.None);

    result.Status.ShouldBe(ResultStatus.Error);
  }

  [Fact]
  public async Task Handle_WhenUnauthenticated_ReturnsCommentsWithoutFollowingStatus()
  {
    var commentAuthor = new Author(Guid.NewGuid(), "commenter", "bio", null);
    var articleAuthor = new Author(Guid.NewGuid(), "writer", "bio", null);
    var article = new Article("Test Title", "desc", "body", articleAuthor);
    var comment = new Comment("test comment", commentAuthor, article);
    article.Comments.Add(comment);

    _articleRepo.FirstOrDefaultAsync(
      Arg.Any<ArticleBySlugSpec>(),
      Arg.Any<CancellationToken>()).Returns(article);

    var handler = new GetCommentsHandler(_articleRepo, _authorRepo);
    var result = await handler.Handle(new GetCommentsQuery("test-title"), CancellationToken.None);

    result.IsSuccess.ShouldBeTrue();
    result.Value.Comments.Count.ShouldBe(1);
    result.Value.Comments[0].Author.Following.ShouldBeFalse();
  }

  [Fact]
  public async Task Handle_WhenAuthenticatedAndFollowing_ReturnsFollowingTrue()
  {
    var commentAuthor = new Author(Guid.NewGuid(), "commenter", "bio", null);
    var articleAuthor = new Author(Guid.NewGuid(), "writer", "bio", null);
    var article = new Article("Test Title", "desc", "body", articleAuthor);
    var comment = new Comment("test comment", commentAuthor, article);
    article.Comments.Add(comment);

    var currentUserId = Guid.NewGuid();
    var currentAuthor = new Author(currentUserId, "reader", "bio", null);
    currentAuthor.Follow(commentAuthor);

    _articleRepo.FirstOrDefaultAsync(
      Arg.Any<ArticleBySlugSpec>(),
      Arg.Any<CancellationToken>()).Returns(article);
    _authorRepo.FirstOrDefaultAsync(
      Arg.Any<AuthorWithFollowingByUserIdSpec>(),
      Arg.Any<CancellationToken>()).Returns(currentAuthor);

    var handler = new GetCommentsHandler(_articleRepo, _authorRepo);
    var result = await handler.Handle(new GetCommentsQuery("test-title", currentUserId), CancellationToken.None);

    result.IsSuccess.ShouldBeTrue();
    result.Value.Comments[0].Author.Following.ShouldBeTrue();
  }
}
