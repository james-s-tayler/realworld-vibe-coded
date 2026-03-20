using Microsoft.Extensions.Logging;
using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Specifications.Articles;
using Server.Core.AuthorAggregate;
using Server.Core.AuthorAggregate.Specifications;
using Server.SharedKernel.Persistence;
using Server.SharedKernel.Result;
using Server.UseCases.Articles.Comments.Create;

namespace Server.UnitTests.UseCases.Articles;

public class CreateCommentHandlerTests
{
  private readonly IRepository<Article> _articleRepo = Substitute.For<IRepository<Article>>();
  private readonly IRepository<Author> _authorRepo = Substitute.For<IRepository<Author>>();
  private readonly ILogger<CreateCommentHandler> _logger = Substitute.For<ILogger<CreateCommentHandler>>();

  [Fact]
  public async Task Handle_WhenArticleNotFound_ReturnsNotFound()
  {
    _articleRepo.FirstOrDefaultAsync(
      Arg.Any<ArticleBySlugSpec>(),
      Arg.Any<CancellationToken>()).Returns((Article?)null);

    var handler = new CreateCommentHandler(_articleRepo, _authorRepo, _logger);
    var result = await handler.Handle(
      new CreateCommentCommand("missing-slug", "body", Guid.NewGuid()), CancellationToken.None);

    result.Status.ShouldBe(ResultStatus.NotFound);
  }

  [Fact]
  public async Task Handle_WhenAuthorNotFound_ReturnsError()
  {
    var articleAuthor = new Author(Guid.NewGuid(), "writer", "bio", null);
    var article = new Article("Test Title", "desc", "body", articleAuthor);

    _articleRepo.FirstOrDefaultAsync(
      Arg.Any<ArticleBySlugSpec>(),
      Arg.Any<CancellationToken>()).Returns(article);
    _authorRepo.FirstOrDefaultAsync(
      Arg.Any<AuthorByUserIdSpec>(),
      Arg.Any<CancellationToken>()).Returns((Author?)null);

    var handler = new CreateCommentHandler(_articleRepo, _authorRepo, _logger);
    var result = await handler.Handle(
      new CreateCommentCommand("test-title", "body", Guid.NewGuid()), CancellationToken.None);

    result.Status.ShouldBe(ResultStatus.Error);
  }

  [Fact]
  public async Task Handle_WhenValid_CreatesCommentAndReturnsCreated()
  {
    var authorId = Guid.NewGuid();
    var articleAuthor = new Author(Guid.NewGuid(), "writer", "bio", null);
    var article = new Article("Test Title", "desc", "body", articleAuthor);
    var commentAuthor = new Author(authorId, "commenter", "bio", null);

    _articleRepo.FirstOrDefaultAsync(
      Arg.Any<ArticleBySlugSpec>(),
      Arg.Any<CancellationToken>()).Returns(article);
    _authorRepo.FirstOrDefaultAsync(
      Arg.Any<AuthorByUserIdSpec>(),
      Arg.Any<CancellationToken>()).Returns(commentAuthor);

    var handler = new CreateCommentHandler(_articleRepo, _authorRepo, _logger);
    var result = await handler.Handle(
      new CreateCommentCommand("test-title", "Great article!", authorId), CancellationToken.None);

    result.Status.ShouldBe(ResultStatus.Created);
    result.Value.Comment.Body.ShouldBe("Great article!");
    await _articleRepo.Received(1).UpdateAsync(article, Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task Handle_WhenAuthenticatedAndFollowing_SetsFollowingTrue()
  {
    var authorId = Guid.NewGuid();
    var currentUserId = Guid.NewGuid();
    var articleAuthor = new Author(Guid.NewGuid(), "writer", "bio", null);
    var article = new Article("Test Title", "desc", "body", articleAuthor);
    var commentAuthor = new Author(authorId, "commenter", "bio", null);
    var currentAuthor = new Author(currentUserId, "reader", "bio", null);
    currentAuthor.Follow(commentAuthor);

    _articleRepo.FirstOrDefaultAsync(
      Arg.Any<ArticleBySlugSpec>(),
      Arg.Any<CancellationToken>()).Returns(article);
    _authorRepo.FirstOrDefaultAsync(
      Arg.Any<AuthorByUserIdSpec>(),
      Arg.Any<CancellationToken>()).Returns(commentAuthor);
    _authorRepo.FirstOrDefaultAsync(
      Arg.Any<AuthorWithFollowingByUserIdSpec>(),
      Arg.Any<CancellationToken>()).Returns(currentAuthor);

    var handler = new CreateCommentHandler(_articleRepo, _authorRepo, _logger);
    var result = await handler.Handle(
      new CreateCommentCommand("test-title", "Nice!", authorId, currentUserId), CancellationToken.None);

    result.Status.ShouldBe(ResultStatus.Created);
    result.Value.Comment.Author.Following.ShouldBeTrue();
  }
}
