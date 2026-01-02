using Server.UseCases.Articles;
using Server.Web.Articles.Create;

namespace Server.FunctionalTests.Articles;

[Collection("Articles Integration Tests")]
public class CreateTests : AppTestBase<ApiFixture>
{
  public CreateTests(ApiFixture fixture) : base(fixture)
  {
  }

  [Fact]
  public async Task CreateArticle_WithValidData_ReturnsArticle()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var request = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Test Article",
        Description = "Test Description",
        Body = "Test Body",
        TagList = new List<string> { "test", "article" },
      },
    };

    var (response, result) = await user.Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(request);

    response.StatusCode.ShouldBe(HttpStatusCode.Created);
    result.Article.ShouldNotBeNull();
    result.Article.Title.ShouldBe("Test Article");
    result.Article.Description.ShouldBe("Test Description");
    result.Article.Body.ShouldBe("Test Body");
    result.Article.Slug.ShouldBe("test-article");
    result.Article.TagList.ShouldContain("test");
    result.Article.TagList.ShouldContain("article");
    result.Article.Author.ShouldNotBeNull();
    result.Article.Author.Username.ShouldBe(user.Email);
    result.Article.Favorited.ShouldBe(false);
    result.Article.FavoritesCount.ShouldBe(0);
  }

  [Fact]
  public async Task CreateArticle_WithoutTags_ReturnsArticle()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var request = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Article Without Tags",
        Description = "Description",
        Body = "Body",
      },
    };

    var (response, result) = await user.Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(request);

    response.StatusCode.ShouldBe(HttpStatusCode.Created);
    result.Article.ShouldNotBeNull();
    result.Article.Title.ShouldBe("Article Without Tags");
    result.Article.TagList.ShouldBeEmpty();
  }

  [Fact]
  public async Task CreateArticle_WithMissingRequiredFields_ReturnsErrorDetail()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var request = new CreateArticleRequest
    {
      Article = new ArticleData(),
    };

    var (response, _) = await user.Client.POSTAsync<Create, CreateArticleRequest, object>(request);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task CreateArticle_WithEmptyRequiredFields_ReturnsErrorDetail()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var request = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = string.Empty,
        Description = string.Empty,
        Body = string.Empty,
      },
    };

    var (response, _) = await user.Client.POSTAsync<Create, CreateArticleRequest, object>(request);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task CreateArticle_WithInvalidTagContainingComma_ReturnsErrorDetail()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var request = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Test",
        Description = "Test",
        Body = "Test",
        TagList = new List<string> { "tag,with,comma" },
      },
    };

    var (response, _) = await user.Client.POSTAsync<Create, CreateArticleRequest, object>(request);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task CreateArticle_WithEmptyTag_ReturnsErrorDetail()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var request = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Test",
        Description = "Test",
        Body = "Test",
        TagList = new List<string> { string.Empty },
      },
    };

    var (response, _) = await user.Client.POSTAsync<Create, CreateArticleRequest, object>(request);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task CreateArticle_WithDuplicateSlug_ReturnsErrorDetail()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var request1 = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Duplicate Title",
        Description = "Description 1",
        Body = "Body 1",
      },
    };

    await user.Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(request1);

    var request2 = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Duplicate Title",
        Description = "Description 2",
        Body = "Body 2",
      },
    };

    var (response, _) = await user.Client.POSTAsync<Create, CreateArticleRequest, object>(request2);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task CreateArticle_WithoutAuthentication_ReturnsUnauthorized()
  {
    var request = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Test",
        Description = "Test",
        Body = "Test",
      },
    };

    var (response, _) = await Fixture.Client.POSTAsync<Create, CreateArticleRequest, object>(request);

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }
}
