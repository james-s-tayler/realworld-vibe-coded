using Server.UseCases.Articles;
using Server.Web.Articles.Create;
using Server.Web.Articles.Get;

namespace Server.FunctionalTests.Articles;

[Collection("Articles Integration Tests")]
public class GetTests : AppTestBase<ApiFixture>
{
  public GetTests(ApiFixture fixture) : base(fixture)
  {
  }

  [Fact]
  public async Task GetArticle_WithAuthentication_ReturnsArticle()
  {
    // Arrange
    var tenant = await Fixture.RegisterTenantAsync();

    var createRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Get Article Test",
        Description = "Description",
        Body = "Body",
      },
    };

    var (response1, createResult) = await tenant.Users[0].Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(createRequest);
    var slug = createResult.Article.Slug;

    // Act
    var (response, result) = await tenant.Users[0].Client.GETAsync<Get, GetArticleRequest, ArticleResponse>(new GetArticleRequest { Slug = slug });

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Article.ShouldNotBeNull();
    result.Article.Slug.ShouldBe(slug);
    result.Article.Title.ShouldBe("Get Article Test");
  }

  [Fact]
  public async Task GetArticle_WithoutAuthentication_ReturnsUnauthorized()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var createRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Get Article Unauthenticated",
        Description = "Description",
        Body = "Body",
      },
    };

    var (_, createResult) = await user.Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(createRequest);
    var slug = createResult.Article.Slug;

    var (response, _) = await Fixture.Client.GETAsync<Get, GetArticleRequest, object>(new GetArticleRequest { Slug = slug });

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task GetArticle_WithNonExistentSlug_ReturnsNotFound()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var (response, _) = await user.Client.GETAsync<Get, GetArticleRequest, object>(new GetArticleRequest { Slug = "no-such-article" });

    response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
  }
}
