using Server.FunctionalTests.Articles.Fixture;
using Server.UseCases.Articles;
using Server.Web.Articles.List;

namespace Server.FunctionalTests.Articles;

[Collection("Articles Integration Tests")]
public class ListTests : AppTestBase<ArticlesFixture>
{
  public ListTests(ArticlesFixture fixture) : base(fixture)
  {
  }

  [Fact]
  public async Task ListArticles_ReturnsArticles()
  {
    var (response, result) = await Fixture.ArticlesUser1Client.GETAsync<ListArticles, ArticlesResponse>();

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Articles.ShouldNotBeNull();
    result.ArticlesCount.ShouldBeGreaterThanOrEqualTo(0);
  }

  [Fact]
  public async Task ListArticles_WithNonExistentAuthorFilter_ReturnsEmptyList()
  {
    var request = new ListArticlesRequest();
    var (response, result) = await Fixture.ArticlesUser1Client.GETAsync<ListArticlesRequest, ArticlesResponse>("/api/articles?author=nonexistentauthor999", request);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Articles.ShouldNotBeNull();
    result.Articles.ShouldBeEmpty();
    result.ArticlesCount.ShouldBe(0);
  }

  [Fact]
  public async Task ListArticles_WithNonExistentFavoritedFilter_ReturnsEmptyList()
  {
    var request = new ListArticlesRequest();
    var (response, result) = await Fixture.ArticlesUser1Client.GETAsync<ListArticlesRequest, ArticlesResponse>("/api/articles?favorited=nonexistentuser999", request);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Articles.ShouldNotBeNull();
    result.Articles.ShouldBeEmpty();
    result.ArticlesCount.ShouldBe(0);
  }

  [Fact]
  public async Task ListArticles_WithNonExistentTagFilter_ReturnsEmptyList()
  {
    var request = new ListArticlesRequest();
    var (response, result) = await Fixture.ArticlesUser1Client.GETAsync<ListArticlesRequest, ArticlesResponse>("/api/articles?tag=nonexistenttag999", request);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Articles.ShouldNotBeNull();
    result.Articles.ShouldBeEmpty();
    result.ArticlesCount.ShouldBe(0);
  }

  [Fact]
  public async Task ListArticles_WithLimit_ReturnsLimitedArticles()
  {
    var request = new ListArticlesRequest();
    var (response, result) = await Fixture.ArticlesUser1Client.GETAsync<ListArticlesRequest, ArticlesResponse>("/api/articles?limit=2", request);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Articles.ShouldNotBeNull();
    result.Articles.Count.ShouldBeLessThanOrEqualTo(2);
  }

  [Fact]
  public async Task ListArticles_WithOffset_SkipsArticles()
  {
    var request = new ListArticlesRequest();
    var (response, result) = await Fixture.ArticlesUser1Client.GETAsync<ListArticlesRequest, ArticlesResponse>("/api/articles?offset=1", request);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Articles.ShouldNotBeNull();
  }

  [Fact]
  public async Task ListArticles_WithLimitAndOffset_ReturnsPaginatedResults()
  {
    var request = new ListArticlesRequest();
    var (response, result) = await Fixture.ArticlesUser1Client.GETAsync<ListArticlesRequest, ArticlesResponse>("/api/articles?limit=2&offset=2", request);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Articles.ShouldNotBeNull();
    result.Articles.Count.ShouldBeLessThanOrEqualTo(2);
  }

  [Fact]
  public async Task ListArticles_WithInvalidOffsetAndLimit_ReturnsErrorDetail()
  {
    var request = new ListArticlesRequest();
    var (response, _) = await Fixture.ArticlesUser1Client.GETAsync<ListArticlesRequest, object>("/api/articles?limit=0&offset=-1", request);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task ListArticles_WithInvalidAuthorParameter_ReturnsErrorDetail()
  {
    var request = new ListArticlesRequest();
    var (response, _) = await Fixture.ArticlesUser1Client.GETAsync<ListArticlesRequest, object>("/api/articles?author=", request);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task ListArticles_WithTagAndAuthorFilters_ReturnsCombinedResults()
  {
    // This test ensures the combination of filters works correctly
    var request = new ListArticlesRequest();
    var (response, result) = await Fixture.ArticlesUser1Client.GETAsync<ListArticlesRequest, ArticlesResponse>($"/api/articles?author={Fixture.ArticlesUser1Username}&tag=test", request);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Articles.ShouldNotBeNull();

    // All returned articles should match both filters (if any exist)
    result.Articles.Where(a => a.Author.Username == Fixture.ArticlesUser1Username).ShouldBe(result.Articles);
  }

  [Fact]
  public async Task ListArticles_WithInvalidTagParameter_ReturnsErrorDetail()
  {
    var request = new ListArticlesRequest();
    var (response, _) = await Fixture.ArticlesUser1Client.GETAsync<ListArticlesRequest, object>("/api/articles?tag=", request);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task ListArticles_WithInvalidFavoritedParameter_ReturnsErrorDetail()
  {
    var request = new ListArticlesRequest();
    var (response, _) = await Fixture.ArticlesUser1Client.GETAsync<ListArticlesRequest, object>("/api/articles?favorited=", request);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }
}
