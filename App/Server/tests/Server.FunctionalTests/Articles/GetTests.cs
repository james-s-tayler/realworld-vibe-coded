using Server.FunctionalTests.Articles.Fixture;
using Server.UseCases.Articles;
using Server.Web.Articles.Create;

namespace Server.FunctionalTests.Articles;

[Collection("Articles Integration Tests")]
public class GetTests(ArticlesFixture App) : TestBase<ArticlesFixture>
{
  [Fact]
  public async Task GetArticle_WithAuthentication_ReturnsArticle()
  {
    var createRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Get Article Test",
        Description = "Description",
        Body = "Body"
      }
    };

    var (_, createResult) = await App.ArticlesUser1Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(createRequest);
    var slug = createResult.Article.Slug;

    var response = await App.ArticlesUser1Client.GetAsync($"/api/articles/{slug}", TestContext.Current.CancellationToken);
    var result = await response.Content.ReadFromJsonAsync<ArticleResponse>(cancellationToken: TestContext.Current.CancellationToken);
    result.ShouldNotBeNull();

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Article.ShouldNotBeNull();
    result.Article.Slug.ShouldBe(slug);
    result.Article.Title.ShouldBe("Get Article Test");
  }

  [Fact]
  public async Task GetArticle_WithoutAuthentication_ReturnsArticle()
  {
    var createRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Get Article Unauthenticated",
        Description = "Description",
        Body = "Body"
      }
    };

    var (_, createResult) = await App.ArticlesUser1Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(createRequest);
    var slug = createResult.Article.Slug;

    var response = await App.Client.GetAsync($"/api/articles/{slug}", TestContext.Current.CancellationToken);
    var result = await response.Content.ReadFromJsonAsync<ArticleResponse>(cancellationToken: TestContext.Current.CancellationToken);
    result.ShouldNotBeNull();

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Article.ShouldNotBeNull();
    result.Article.Slug.ShouldBe(slug);
  }

  [Fact]
  public async Task GetArticle_WithNonExistentSlug_ReturnsNotFound()
  {
    var response = await App.Client.GetAsync("/api/articles/no-such-article", TestContext.Current.CancellationToken);

    response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
  }
}
