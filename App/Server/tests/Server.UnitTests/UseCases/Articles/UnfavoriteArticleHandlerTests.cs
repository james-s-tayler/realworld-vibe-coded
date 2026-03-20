using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Specifications.Articles;
using Server.Core.AuthorAggregate;
using Server.Core.AuthorAggregate.Specifications;
using Server.SharedKernel.Persistence;
using Server.SharedKernel.Result;
using Server.UseCases.Articles.Unfavorite;

namespace Server.UnitTests.UseCases.Articles;

public class UnfavoriteArticleHandlerTests
{
  private readonly IRepository<Article> _articleRepo = Substitute.For<IRepository<Article>>();
  private readonly IRepository<Author> _authorRepo = Substitute.For<IRepository<Author>>();

  [Fact]
  public async Task Handle_WhenArticleNotFound_ReturnsNotFound()
  {
    _articleRepo.FirstOrDefaultAsync(
      Arg.Any<ArticleBySlugSpec>(),
      Arg.Any<CancellationToken>()).Returns((Article?)null);

    var handler = new UnfavoriteArticleHandler(_articleRepo, _authorRepo);
    var result = await handler.Handle(
      new UnfavoriteArticleCommand("missing-slug", Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

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

    var handler = new UnfavoriteArticleHandler(_articleRepo, _authorRepo);
    var result = await handler.Handle(
      new UnfavoriteArticleCommand("test-title", Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

    result.Status.ShouldBe(ResultStatus.Error);
  }

  [Fact]
  public async Task Handle_WhenValid_RemovesFromFavoritesAndReturnsSuccess()
  {
    var articleAuthor = new Author(Guid.NewGuid(), "writer", "bio", null);
    var article = new Article("Test Title", "desc", "body", articleAuthor);
    var unfavoritingAuthor = new Author(Guid.NewGuid(), "reader", "bio", null);
    article.AddToFavorites(unfavoritingAuthor);

    _articleRepo.FirstOrDefaultAsync(
      Arg.Any<ArticleBySlugSpec>(),
      Arg.Any<CancellationToken>()).Returns(article);
    _authorRepo.FirstOrDefaultAsync(
      Arg.Any<AuthorByUserIdSpec>(),
      Arg.Any<CancellationToken>()).Returns(unfavoritingAuthor);

    var handler = new UnfavoriteArticleHandler(_articleRepo, _authorRepo);
    var result = await handler.Handle(
      new UnfavoriteArticleCommand("test-title", Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

    result.IsSuccess.ShouldBeTrue();
    article.FavoritedBy.ShouldNotContain(unfavoritingAuthor);
    await _articleRepo.Received(1).UpdateAsync(article, Arg.Any<CancellationToken>());
  }
}
