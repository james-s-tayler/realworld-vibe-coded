using Server.FunctionalTests.Articles.Fixture;
using Server.UseCases.Articles;
using Server.Web.Articles.List;

namespace Server.FunctionalTests.Articles;

[Collection("Articles Integration Tests")]
public class ListTests(ArticlesFixture App) : TestBase<ArticlesFixture>
{
  [Fact]
  public async Task ListArticles_ReturnsArticles()
  {
    var (response, result) = await App.Client.GETAsync<ListArticles, ArticlesResponse>();

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Articles.ShouldNotBeNull();
    result.ArticlesCount.ShouldBeGreaterThanOrEqualTo(0);
  }

  [Fact]
  public async Task ListArticles_WithNonExistentAuthorFilter_ReturnsEmptyList()
  {
    var request = new ListArticlesRequest();
    var (response, result) = await App.Client.GETAsync<ListArticlesRequest, ArticlesResponse>("/api/articles?author=nonexistentauthor999", request);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Articles.ShouldNotBeNull();
    result.Articles.ShouldBeEmpty();
    result.ArticlesCount.ShouldBe(0);
  }

  [Fact]
  public async Task ListArticles_WithNonExistentFavoritedFilter_ReturnsEmptyList()
  {
    var request = new ListArticlesRequest();
    var (response, result) = await App.Client.GETAsync<ListArticlesRequest, ArticlesResponse>("/api/articles?favorited=nonexistentuser999", request);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Articles.ShouldNotBeNull();
    result.Articles.ShouldBeEmpty();
    result.ArticlesCount.ShouldBe(0);
  }

  [Fact]
  public async Task ListArticles_WithNonExistentTagFilter_ReturnsEmptyList()
  {
    var request = new ListArticlesRequest();
    var (response, result) = await App.Client.GETAsync<ListArticlesRequest, ArticlesResponse>("/api/articles?tag=nonexistenttag999", request);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Articles.ShouldNotBeNull();
    result.Articles.ShouldBeEmpty();
    result.ArticlesCount.ShouldBe(0);
  }

  [Fact]
  public async Task ListArticles_WithLimit_ReturnsLimitedArticles()
  {
    var request = new ListArticlesRequest();
    var (response, result) = await App.Client.GETAsync<ListArticlesRequest, ArticlesResponse>("/api/articles?limit=2", request);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Articles.ShouldNotBeNull();
    result.Articles.Count.ShouldBeLessThanOrEqualTo(2);
  }

  [Fact]
  public async Task ListArticles_WithOffset_SkipsArticles()
  {
    var request = new ListArticlesRequest();
    var (response, result) = await App.Client.GETAsync<ListArticlesRequest, ArticlesResponse>("/api/articles?offset=1", request);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Articles.ShouldNotBeNull();
  }

  [Fact]
  public async Task ListArticles_WithLimitAndOffset_ReturnsPaginatedResults()
  {
    var request = new ListArticlesRequest();
    var (response, result) = await App.Client.GETAsync<ListArticlesRequest, ArticlesResponse>("/api/articles?limit=2&offset=2", request);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Articles.ShouldNotBeNull();
    result.Articles.Count.ShouldBeLessThanOrEqualTo(2);
  }

  [Fact]
  public async Task ListArticles_WithInvalidOffsetAndLimit_ReturnsValidationError()
  {
    var request = new ListArticlesRequest();
    var (response, _) = await App.Client.GETAsync<ListArticlesRequest, object>("/api/articles?limit=0&offset=-1", request);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task ListArticles_WithInvalidAuthorParameter_ReturnsValidationError()
  {
    var request = new ListArticlesRequest();
    var (response, _) = await App.Client.GETAsync<ListArticlesRequest, object>("/api/articles?author=", request);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task ListArticles_WithInvalidTagParameter_ReturnsValidationError()
  {
    var request = new ListArticlesRequest();
    var (response, _) = await App.Client.GETAsync<ListArticlesRequest, object>("/api/articles?tag=", request);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task ListArticles_WithInvalidFavoritedParameter_ReturnsValidationError()
  {
    var request = new ListArticlesRequest();
    var (response, _) = await App.Client.GETAsync<ListArticlesRequest, object>("/api/articles?favorited=", request);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }
}
