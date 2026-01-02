#pragma warning disable xUnit1051

using Server.FunctionalTests.Articles.Fixture;
using Server.UseCases.Articles;
using Server.UseCases.Tags;
using Server.Web.Articles.List;
using Server.Web.Tags.List;

namespace Server.FunctionalTests.Articles;

[Collection("Empty Articles Integration Tests")]
public class EmptyArticlesTests : AppTestBase<EmptyArticlesFixture>
{
  public EmptyArticlesTests(EmptyArticlesFixture fixture) : base(fixture)
  {
  }

  [Fact]
  public async Task AllArticles_WhenEmpty_ReturnsEmptyList()
  {
    // Arrange
    var tenant = await Fixture.RegisterTenantAsync();

    // Act
    var (response, result) = await tenant.Users.First().Client.GETAsync<ListArticles, ArticlesResponse>();

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Articles.ShouldNotBeNull();
    result.Articles.ShouldBeEmpty();
    result.ArticlesCount.ShouldBe(0);
  }

  [Fact]
  public async Task ArticlesByAuthor_WhenEmpty_ReturnsEmptyList()
  {
    // Arrange
    var tenant = await Fixture.RegisterTenantAsync();

    var request = new ListArticlesRequest();

    // Act
    var (response, result) = await tenant.Users.First().Client.GETAsync<ListArticlesRequest, ArticlesResponse>("/api/articles?author=johnjacob", request);

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Articles.ShouldNotBeNull();
    result.Articles.ShouldBeEmpty();
    result.ArticlesCount.ShouldBe(0);
  }

  [Fact]
  public async Task ArticlesFavoritedByUsername_WhenEmpty_ReturnsEmptyList()
  {
    // Arrange
    var tenant = await Fixture.RegisterTenantAsync();

    var request = new ListArticlesRequest();

    // Act
    var (response, result) = await tenant.Users.First().Client.GETAsync<ListArticlesRequest, ArticlesResponse>("/api/articles?favorited=testuser", request);

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Articles.ShouldNotBeNull();
    result.Articles.ShouldBeEmpty();
    result.ArticlesCount.ShouldBe(0);
  }

  [Fact]
  public async Task ArticlesByTag_WhenEmpty_ReturnsEmptyList()
  {
    // Arrange
    var tenant = await Fixture.RegisterTenantAsync();

    var request = new ListArticlesRequest();

    // Act
    var (response, result) = await tenant.Users.First().Client.GETAsync<ListArticlesRequest, ArticlesResponse>("/api/articles?tag=dragons", request);

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Articles.ShouldNotBeNull();
    result.Articles.ShouldBeEmpty();
    result.ArticlesCount.ShouldBe(0);
  }

  [Fact]
  public async Task GetTags_WhenEmpty_ReturnsEmptyList()
  {
    // Arrange
    var tenant = await Fixture.RegisterTenantAsync();

    // Act
    var (response, result) = await tenant.Users.First().Client.GETAsync<List, TagsResponse>();

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Tags.ShouldNotBeNull();
    result.Tags.ShouldBeEmpty();
  }
}
