#pragma warning disable xUnit1051

using Server.FunctionalTests.Articles.Fixture;
using Server.UseCases.Articles;
using Server.UseCases.Tags;
using Server.Web.Articles.List;
using Server.Web.Tags.List;

namespace Server.FunctionalTests.Articles;

[Collection("Empty Articles Integration Tests")]
public class EmptyArticlesTests(EmptyArticlesFixture App) : TestBase<EmptyArticlesFixture>
{
  [Fact]
  public async Task AllArticles_WhenEmpty_ReturnsEmptyList()
  {
    var (response, result) = await App.Client.GETAsync<ListArticles, ArticlesResponse>();

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Articles.ShouldNotBeNull();
    result.Articles.ShouldBeEmpty();
    result.ArticlesCount.ShouldBe(0);
  }

  [Fact]
  public async Task ArticlesByAuthor_WhenEmpty_ReturnsEmptyList()
  {
    // SRV007: Using raw HttpClient.GetAsync is acceptable here for this existing test
    // that was written before the analyzer was introduced.
#pragma warning disable SRV007
    var response = await App.Client.GetAsync("/api/articles?author=johnjacob");
    var result = await response.Content.ReadFromJsonAsync<ArticlesResponse>();
#pragma warning restore SRV007
    result.ShouldNotBeNull();

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Articles.ShouldNotBeNull();
    result.Articles.ShouldBeEmpty();
    result.ArticlesCount.ShouldBe(0);
  }

  [Fact]
  public async Task ArticlesFavoritedByUsername_WhenEmpty_ReturnsEmptyList()
  {
    // SRV007: Using raw HttpClient.GetAsync is acceptable here for this existing test
    // that was written before the analyzer was introduced.
#pragma warning disable SRV007
    var response = await App.Client.GetAsync("/api/articles?favorited=testuser");
    var result = await response.Content.ReadFromJsonAsync<ArticlesResponse>();
#pragma warning restore SRV007
    result.ShouldNotBeNull();

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Articles.ShouldNotBeNull();
    result.Articles.ShouldBeEmpty();
    result.ArticlesCount.ShouldBe(0);
  }

  [Fact]
  public async Task ArticlesByTag_WhenEmpty_ReturnsEmptyList()
  {
    // SRV007: Using raw HttpClient.GetAsync is acceptable here for this existing test
    // that was written before the analyzer was introduced.
#pragma warning disable SRV007
    var response = await App.Client.GetAsync("/api/articles?tag=dragons");
    var result = await response.Content.ReadFromJsonAsync<ArticlesResponse>();
#pragma warning restore SRV007
    result.ShouldNotBeNull();

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Articles.ShouldNotBeNull();
    result.Articles.ShouldBeEmpty();
    result.ArticlesCount.ShouldBe(0);
  }

  [Fact]
  public async Task GetTags_WhenEmpty_ReturnsEmptyList()
  {
    var (response, result) = await App.Client.GETAsync<List, TagsResponse>();

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Tags.ShouldNotBeNull();
    result.Tags.ShouldBeEmpty();
  }
}
