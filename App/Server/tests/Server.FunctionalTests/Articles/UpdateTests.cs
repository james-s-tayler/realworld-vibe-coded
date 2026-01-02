using Server.UseCases.Articles;
using Server.Web.Articles.Create;
using Server.Web.Articles.Update;

namespace Server.FunctionalTests.Articles;

public class UpdateTests : AppTestBase
{
  public UpdateTests(ApiFixture apiFixture, ITestOutputHelper output) : base(apiFixture, output)
  {
  }

  [Fact]
  public async Task UpdateArticle_WithValidData_ReturnsUpdatedArticle()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var createRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Original Title",
        Description = "Original Description",
        Body = "Original Body",
      },
    };

    var (_, createResult) = await user.Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(createRequest);
    var slug = createResult.Article.Slug;

    var updateRequest = new UpdateArticleRequest
    {
      Slug = slug,
      Article = new UpdateArticleData
      {
        Title = "Updated Title",
        Description = "Updated Description",
        Body = "Updated Body",
      },
    };

    var (response, result) = await user.Client.PUTAsync<Update, UpdateArticleRequest, ArticleResponse>(updateRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Article.ShouldNotBeNull();
    result.Article.Title.ShouldBe("Updated Title");
    result.Article.Description.ShouldBe("Updated Description");
    result.Article.Body.ShouldBe("Updated Body");
    result.Article.Slug.ShouldBe("updated-title");
  }

  [Fact]
  public async Task UpdateArticle_WithoutAuthentication_ReturnsUnauthorized()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var createRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Update Unauth Test",
        Description = "Description",
        Body = "Body",
      },
    };

    var (_, createResult) = await user.Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(createRequest);
    var slug = createResult.Article.Slug;

    var updateRequest = new UpdateArticleRequest
    {
      Slug = slug,
      Article = new UpdateArticleData
      {
        Title = "Updated",
      },
    };

    var (response, _) = await Fixture.Client.PUTAsync<Update, UpdateArticleRequest, object>(updateRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task UpdateArticle_WithNonExistentArticle_ReturnsNotFound()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var updateRequest = new UpdateArticleRequest
    {
      Slug = "no-such-article",
      Article = new UpdateArticleData
      {
        Title = "Updated",
      },
    };

    var (response, _) = await user.Client.PUTAsync<Update, UpdateArticleRequest, object>(updateRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task UpdateArticle_ByWrongUser_ReturnsForbidden()
  {
    // Arrange
    var tenant = await Fixture.RegisterTenantWithUsersAsync(2);

    var createRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Update Wrong User Test",
        Description = "Description",
        Body = "Body",
      },
    };

    var (_, createResult) = await tenant.Users[0].Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(createRequest);
    var slug = createResult.Article.Slug;

    var updateRequest = new UpdateArticleRequest
    {
      Slug = slug,
      Article = new UpdateArticleData
      {
        Title = "Updated by wrong user",
      },
    };

    // Act
    var (response, _) = await tenant.Users[1].Client.PUTAsync<Update, UpdateArticleRequest, object>(updateRequest);

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
  }

  [Fact]
  public async Task UpdateArticle_WithAllFieldsEmpty_ReturnsErrorDetail()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var createRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Update Empty Fields Test",
        Description = "Description",
        Body = "Body",
      },
    };

    var (_, createResult) = await user.Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(createRequest);
    var slug = createResult.Article.Slug;

    var updateRequest = new UpdateArticleRequest
    {
      Slug = slug,
      Article = new UpdateArticleData
      {
        Title = string.Empty,
        Description = string.Empty,
        Body = string.Empty,
      },
    };

    var (response, _) = await user.Client.PUTAsync<Update, UpdateArticleRequest, object>(updateRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }
}
