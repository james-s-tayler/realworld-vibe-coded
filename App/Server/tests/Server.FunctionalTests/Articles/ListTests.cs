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
}
