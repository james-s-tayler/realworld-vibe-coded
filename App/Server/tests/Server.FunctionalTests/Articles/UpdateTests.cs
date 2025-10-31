using Server.FunctionalTests.Articles.Fixture;
using Server.UseCases.Articles;
using Server.Web.Articles.Create;
using Server.Web.Articles.Update;

namespace Server.FunctionalTests.Articles;

[Collection("Articles Integration Tests")]
public class UpdateTests(ArticlesFixture App) : TestBase<ArticlesFixture>
{
  [Fact]
  public async Task UpdateArticle_WithValidData_ReturnsUpdatedArticle()
  {
    var createRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Original Title",
        Description = "Original Description",
        Body = "Original Body"
      }
    };

    var (_, createResult) = await App.ArticlesUser1Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(createRequest);
    var slug = createResult.Article.Slug;

    var updateRequest = new UpdateArticleRequest
    {
      Slug = slug,
      Article = new UpdateArticleData
      {
        Title = "Updated Title",
        Description = "Updated Description",
        Body = "Updated Body"
      }
    };

    var (response, result) = await App.ArticlesUser1Client.PUTAsync<Update, UpdateArticleRequest, ArticleResponse>(updateRequest);

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
    var createRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Update Unauth Test",
        Description = "Description",
        Body = "Body"
      }
    };

    var (_, createResult) = await App.ArticlesUser1Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(createRequest);
    var slug = createResult.Article.Slug;

    var updateRequest = new UpdateArticleRequest
    {
      Slug = slug,
      Article = new UpdateArticleData
      {
        Title = "Updated"
      }
    };

    var (response, _) = await App.Client.PUTAsync<Update, UpdateArticleRequest, object>(updateRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task UpdateArticle_WithNonExistentArticle_ReturnsNotFound()
  {
    var updateRequest = new UpdateArticleRequest
    {
      Slug = "no-such-article",
      Article = new UpdateArticleData
      {
        Title = "Updated"
      }
    };

    var (response, _) = await App.ArticlesUser1Client.PUTAsync<Update, UpdateArticleRequest, object>(updateRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task UpdateArticle_ByWrongUser_ReturnsForbidden()
  {
    var createRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Update Wrong User Test",
        Description = "Description",
        Body = "Body"
      }
    };

    var (_, createResult) = await App.ArticlesUser1Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(createRequest);
    var slug = createResult.Article.Slug;

    var updateRequest = new UpdateArticleRequest
    {
      Slug = slug,
      Article = new UpdateArticleData
      {
        Title = "Updated by wrong user"
      }
    };

    var (response, _) = await App.ArticlesUser2Client.PUTAsync<Update, UpdateArticleRequest, object>(updateRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
  }

  [Fact]
  public async Task UpdateArticle_WithAllFieldsEmpty_ReturnsErrorDetail()
  {
    var createRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Update Empty Fields Test",
        Description = "Description",
        Body = "Body"
      }
    };

    var (_, createResult) = await App.ArticlesUser1Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(createRequest);
    var slug = createResult.Article.Slug;

    var updateRequest = new UpdateArticleRequest
    {
      Slug = slug,
      Article = new UpdateArticleData
      {
        Title = "",
        Description = "",
        Body = ""
      }
    };

    var (response, _) = await App.ArticlesUser1Client.PUTAsync<Update, UpdateArticleRequest, object>(updateRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }
}
