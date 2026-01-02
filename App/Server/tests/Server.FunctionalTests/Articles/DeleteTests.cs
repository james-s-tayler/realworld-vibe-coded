using Server.FunctionalTests.Articles.Fixture;
using Server.UseCases.Articles;
using Server.Web.Articles.Create;
using Server.Web.Articles.Delete;

namespace Server.FunctionalTests.Articles;

[Collection("Articles Integration Tests")]
public class DeleteTests : AppTestBase<ArticlesFixture>
{
  public DeleteTests(ArticlesFixture fixture) : base(fixture)
  {
  }

  [Fact]
  public async Task DeleteArticle_WithAuthentication_ReturnsNoContent()
  {
    var createRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Delete Article Test",
        Description = "Description",
        Body = "Body",
      },
    };

    var (_, createResult) = await Fixture.ArticlesUser1Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(createRequest);
    var slug = createResult.Article.Slug;

    var (response, _) = await Fixture.ArticlesUser1Client.DELETEAsync<Delete, DeleteArticleRequest, object>(new DeleteArticleRequest { Slug = slug });

    response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
  }

  [Fact]
  public async Task DeleteArticle_WithNonExistentArticle_ReturnsNotFound()
  {
    var (response, _) = await Fixture.ArticlesUser1Client.DELETEAsync<Delete, DeleteArticleRequest, object>(new DeleteArticleRequest { Slug = "no-such-article" });

    response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task DeleteArticle_WithoutAuthentication_ReturnsUnauthorized()
  {
    var createRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Delete Unauth Test",
        Description = "Description",
        Body = "Body",
      },
    };

    var (_, createResult) = await Fixture.ArticlesUser1Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(createRequest);
    var slug = createResult.Article.Slug;

    var (response, _) = await Fixture.Client.DELETEAsync<Delete, DeleteArticleRequest, object>(new DeleteArticleRequest { Slug = slug });

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task DeleteArticle_ByWrongUser_ReturnsForbidden()
  {
    // Arrange
    var tenant = await Fixture.RegisterTenantWithUsersAsync(2);

    var createRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Delete Wrong User Test",
        Description = "Description",
        Body = "Body",
      },
    };

    var (_, createResult) = await tenant.Users[0].Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(createRequest);
    var slug = createResult.Article.Slug;

    // Act
    var (response, _) = await tenant.Users[1].Client.DELETEAsync<Delete, DeleteArticleRequest, object>(new DeleteArticleRequest { Slug = slug });

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
  }
}
