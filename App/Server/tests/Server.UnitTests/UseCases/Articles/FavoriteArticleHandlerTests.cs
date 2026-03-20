using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Specifications.Articles;
using Server.Core.AuthorAggregate;
using Server.Core.AuthorAggregate.Specifications;
using Server.SharedKernel.Persistence;
using Server.SharedKernel.Result;
using Server.UseCases.Articles.Favorite;

namespace Server.UnitTests.UseCases.Articles;

public class FavoriteArticleHandlerTests
{
  private readonly IRepository<Article> _articleRepo = Substitute.For<IRepository<Article>>();
  private readonly IRepository<Author> _authorRepo = Substitute.For<IRepository<Author>>();

  [Fact]
  public async Task Handle_WhenArticleNotFound_ReturnsNotFound()
  {
    _articleRepo.FirstOrDefaultAsync(
      Arg.Any<ArticleBySlugSpec>(),
      Arg.Any<CancellationToken>()).Returns((Article?)null);

    var handler = new FavoriteArticleHandler(_articleRepo, _authorRepo);
    var result = await handler.Handle(
      new FavoriteArticleCommand("missing-slug", Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

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

    var handler = new FavoriteArticleHandler(_articleRepo, _authorRepo);
    var result = await handler.Handle(
      new FavoriteArticleCommand("test-title", Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

    result.Status.ShouldBe(ResultStatus.Error);
  }

  [Fact]
  public async Task Handle_WhenValid_AddsToFavoritesAndReturnsSuccess()
  {
    var articleAuthor = new Author(Guid.NewGuid(), "writer", "bio", null);
    var article = new Article("Test Title", "desc", "body", articleAuthor);
    var favoritingAuthor = new Author(Guid.NewGuid(), "reader", "bio", null);

    _articleRepo.FirstOrDefaultAsync(
      Arg.Any<ArticleBySlugSpec>(),
      Arg.Any<CancellationToken>()).Returns(article);
    _authorRepo.FirstOrDefaultAsync(
      Arg.Any<AuthorByUserIdSpec>(),
      Arg.Any<CancellationToken>()).Returns(favoritingAuthor);

    var handler = new FavoriteArticleHandler(_articleRepo, _authorRepo);
    var result = await handler.Handle(
      new FavoriteArticleCommand("test-title", Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

    result.IsSuccess.ShouldBeTrue();
    article.FavoritedBy.ShouldContain(favoritingAuthor);
    await _articleRepo.Received(1).UpdateAsync(article, Arg.Any<CancellationToken>());
  }
}
