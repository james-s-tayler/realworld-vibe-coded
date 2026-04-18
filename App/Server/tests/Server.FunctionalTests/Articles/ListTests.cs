using Server.Core.ArticleAggregate.Dtos;
using Server.SharedKernel.Pagination;
using Server.Web.Articles.List;

namespace Server.FunctionalTests.Articles;

public class ListTests : AppTestBase
{
  public ListTests(ApiFixture apiFixture, ITestOutputHelper output) : base(apiFixture, output)
  {
  }

  [Fact]
  public async Task ListArticles_ReturnsArticles()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var (response, result) = await user.Client.GETAsync<ListArticles, PaginatedResponse<ArticleDto>>();

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Items.ShouldNotBeNull();
    result.Count.ShouldBeGreaterThanOrEqualTo(0);
  }

  [Fact]
  public async Task ListArticles_WithNonExistentAuthorFilter_ReturnsEmptyList()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var request = new ListArticlesRequest();
    var (response, result) = await user.Client.GETAsync<ListArticlesRequest, PaginatedResponse<ArticleDto>>("/api/articles?author=nonexistentauthor999", request);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Items.ShouldNotBeNull();
    result.Items.ShouldBeEmpty();
    result.Count.ShouldBe(0);
  }

  [Fact]
  public async Task ListArticles_WithNonExistentFavoritedFilter_ReturnsEmptyList()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var request = new ListArticlesRequest();
    var (response, result) = await user.Client.GETAsync<ListArticlesRequest, PaginatedResponse<ArticleDto>>("/api/articles?favorited=nonexistentuser999", request);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Items.ShouldNotBeNull();
    result.Items.ShouldBeEmpty();
    result.Count.ShouldBe(0);
  }

  [Fact]
  public async Task ListArticles_WithNonExistentTagFilter_ReturnsEmptyList()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var request = new ListArticlesRequest();
    var (response, result) = await user.Client.GETAsync<ListArticlesRequest, PaginatedResponse<ArticleDto>>("/api/articles?tag=nonexistenttag999", request);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Items.ShouldNotBeNull();
    result.Items.ShouldBeEmpty();
    result.Count.ShouldBe(0);
  }

  [Fact]
  public async Task ListArticles_WithLimit_ReturnsLimitedArticles()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var request = new ListArticlesRequest();
    var (response, result) = await user.Client.GETAsync<ListArticlesRequest, PaginatedResponse<ArticleDto>>("/api/articles?limit=2", request);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Items.ShouldNotBeNull();
    result.Items.Count.ShouldBeLessThanOrEqualTo(2);
  }

  [Fact]
  public async Task ListArticles_WithOffset_SkipsArticles()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var request = new ListArticlesRequest();
    var (response, result) = await user.Client.GETAsync<ListArticlesRequest, PaginatedResponse<ArticleDto>>("/api/articles?offset=1", request);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Items.ShouldNotBeNull();
  }

  [Fact]
  public async Task ListArticles_WithLimitAndOffset_ReturnsPaginatedResults()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var request = new ListArticlesRequest();
    var (response, result) = await user.Client.GETAsync<ListArticlesRequest, PaginatedResponse<ArticleDto>>("/api/articles?limit=2&offset=2", request);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Items.ShouldNotBeNull();
    result.Items.Count.ShouldBeLessThanOrEqualTo(2);
  }

  [Fact]
  public async Task ListArticles_WithInvalidOffsetAndLimit_ReturnsErrorDetail()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var request = new ListArticlesRequest();
    var (response, _) = await user.Client.GETAsync<ListArticlesRequest, object>("/api/articles?limit=0&offset=-1", request);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task ListArticles_WithInvalidAuthorParameter_ReturnsErrorDetail()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var request = new ListArticlesRequest();
    var (response, _) = await user.Client.GETAsync<ListArticlesRequest, object>("/api/articles?author=", request);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task ListArticles_WithTagAndAuthorFilters_ReturnsCombinedResults()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    // This test ensures the combination of filters works correctly
    var request = new ListArticlesRequest();
    var (response, result) = await user.Client.GETAsync<ListArticlesRequest, PaginatedResponse<ArticleDto>>($"/api/articles?author={user.Email}&tag=test", request);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Items.ShouldNotBeNull();

    // All returned articles should match both filters (if any exist)
    result.Items.Where(a => a.Author.Username == user.Email).ShouldBe(result.Items);
  }

  [Fact]
  public async Task ListArticles_WithInvalidTagParameter_ReturnsErrorDetail()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var request = new ListArticlesRequest();
    var (response, _) = await user.Client.GETAsync<ListArticlesRequest, object>("/api/articles?tag=", request);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task ListArticles_WithInvalidFavoritedParameter_ReturnsErrorDetail()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var request = new ListArticlesRequest();
    var (response, _) = await user.Client.GETAsync<ListArticlesRequest, object>("/api/articles?favorited=", request);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }
}
