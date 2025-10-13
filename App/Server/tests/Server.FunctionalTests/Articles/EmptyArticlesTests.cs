#pragma warning disable xUnit1051

using System.Net;
using System.Net.Http.Json;
using Server.UseCases.Articles;
using Server.UseCases.Tags;

namespace Server.FunctionalTests.Articles;

[Collection("Empty Articles Integration Tests")]
public class EmptyArticlesTests(EmptyArticlesFixture App) : TestBase<EmptyArticlesFixture>
{
  [Fact]
  public async Task AllArticles_WhenEmpty_ReturnsEmptyList()
  {
    var (response, result) = await App.Client.GETAsync<Server.Web.Articles.List, ArticlesResponse>();

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Articles.ShouldNotBeNull();
    result.Articles.ShouldBeEmpty();
    result.ArticlesCount.ShouldBe(0);
  }

  [Fact]
  public async Task ArticlesByAuthor_WhenEmpty_ReturnsEmptyList()
  {
    var response = await App.Client.GetAsync("/api/articles?author=johnjacob");
    var result = await response.Content.ReadFromJsonAsync<ArticlesResponse>();
    result.ShouldNotBeNull();

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Articles.ShouldNotBeNull();
    result.Articles.ShouldBeEmpty();
    result.ArticlesCount.ShouldBe(0);
  }

  [Fact]
  public async Task ArticlesFavoritedByUsername_WhenEmpty_ReturnsEmptyList()
  {
    var response = await App.Client.GetAsync("/api/articles?favorited=testuser");
    var result = await response.Content.ReadFromJsonAsync<ArticlesResponse>();
    result.ShouldNotBeNull();

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Articles.ShouldNotBeNull();
    result.Articles.ShouldBeEmpty();
    result.ArticlesCount.ShouldBe(0);
  }

  [Fact]
  public async Task ArticlesByTag_WhenEmpty_ReturnsEmptyList()
  {
    var response = await App.Client.GetAsync("/api/articles?tag=dragons");
    var result = await response.Content.ReadFromJsonAsync<ArticlesResponse>();
    result.ShouldNotBeNull();

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Articles.ShouldNotBeNull();
    result.Articles.ShouldBeEmpty();
    result.ArticlesCount.ShouldBe(0);
  }

  [Fact]
  public async Task GetTags_WhenEmpty_ReturnsEmptyList()
  {
    var (response, result) = await App.Client.GETAsync<Server.Web.Tags.List, TagsResponse>();

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Tags.ShouldNotBeNull();
    result.Tags.ShouldBeEmpty();
  }
}
