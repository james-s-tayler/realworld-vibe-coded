#pragma warning disable xUnit1051

using Server.Core.ArticleAggregate.Dtos;
using Server.SharedKernel.Pagination;
using Server.UseCases.Tags;
using Server.Web.Articles.List;
using Server.Web.Tags.List;

namespace Server.FunctionalTests.Articles;

public class EmptyArticlesTests : AppTestBase
{
  public EmptyArticlesTests(ApiFixture apiFixture, ITestOutputHelper output) : base(apiFixture, output)
  {
  }

  [Fact]
  public async Task AllArticles_WhenEmpty_ReturnsEmptyList()
  {
    // Arrange
    var tenant = await Fixture.RegisterTenantAsync();

    // Act
    var (response, result) = await tenant.Users.First().Client.GETAsync<ListArticles, PaginatedResponse<ArticleDto>>();

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Items.ShouldNotBeNull();
    result.Items.ShouldBeEmpty();
    result.Count.ShouldBe(0);
  }

  [Fact]
  public async Task ArticlesByAuthor_WhenEmpty_ReturnsEmptyList()
  {
    // Arrange
    var tenant = await Fixture.RegisterTenantAsync();

    var request = new ListArticlesRequest();

    // Act
    var (response, result) = await tenant.Users.First().Client.GETAsync<ListArticlesRequest, PaginatedResponse<ArticleDto>>("/api/articles?author=johnjacob", request);

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Items.ShouldNotBeNull();
    result.Items.ShouldBeEmpty();
    result.Count.ShouldBe(0);
  }

  [Fact]
  public async Task ArticlesFavoritedByUsername_WhenEmpty_ReturnsEmptyList()
  {
    // Arrange
    var tenant = await Fixture.RegisterTenantAsync();

    var request = new ListArticlesRequest();

    // Act
    var (response, result) = await tenant.Users.First().Client.GETAsync<ListArticlesRequest, PaginatedResponse<ArticleDto>>("/api/articles?favorited=testuser", request);

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Items.ShouldNotBeNull();
    result.Items.ShouldBeEmpty();
    result.Count.ShouldBe(0);
  }

  [Fact]
  public async Task ArticlesByTag_WhenEmpty_ReturnsEmptyList()
  {
    // Arrange
    var tenant = await Fixture.RegisterTenantAsync();

    var request = new ListArticlesRequest();

    // Act
    var (response, result) = await tenant.Users.First().Client.GETAsync<ListArticlesRequest, PaginatedResponse<ArticleDto>>("/api/articles?tag=dragons", request);

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Items.ShouldNotBeNull();
    result.Items.ShouldBeEmpty();
    result.Count.ShouldBe(0);
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
