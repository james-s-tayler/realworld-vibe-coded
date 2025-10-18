using Server.FunctionalTests.Articles.Fixture;
using Server.UseCases.Articles;
using Server.Web.Articles.Create;

namespace Server.FunctionalTests.Articles;

[Collection("Articles Integration Tests")]
public class UnfavoriteTests(ArticlesFixture App) : TestBase<ArticlesFixture>
{
  [Fact]
  public async Task UnfavoriteArticle_WithAuthentication_ReturnsArticleWithoutFavorite()
  {
    var createRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Unfavorite Article Test",
        Description = "Description",
        Body = "Body"
      }
    };

    var (_, createResult) = await App.ArticlesUser1Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(createRequest);
    var slug = createResult.Article.Slug;

    await App.ArticlesUser1Client.PostAsync($"/api/articles/{slug}/favorite", null, TestContext.Current.CancellationToken);

    var response = await App.ArticlesUser1Client.DeleteAsync($"/api/articles/{slug}/favorite", TestContext.Current.CancellationToken);
    var result = await response.Content.ReadFromJsonAsync<ArticleResponse>(cancellationToken: TestContext.Current.CancellationToken);
    result.ShouldNotBeNull();

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Article.ShouldNotBeNull();
    result.Article.Favorited.ShouldBe(false);
    result.Article.FavoritesCount.ShouldBe(0);
  }

  [Fact]
  public async Task UnfavoriteArticle_NotAlreadyFavorited_ReturnsArticleWithoutFavorite()
  {
    var createRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Not Favorited Test",
        Description = "Description",
        Body = "Body"
      }
    };

    var (_, createResult) = await App.ArticlesUser1Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(createRequest);
    var slug = createResult.Article.Slug;

    var response = await App.ArticlesUser1Client.DeleteAsync($"/api/articles/{slug}/favorite", TestContext.Current.CancellationToken);
    var result = await response.Content.ReadFromJsonAsync<ArticleResponse>(cancellationToken: TestContext.Current.CancellationToken);
    result.ShouldNotBeNull();

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Article.Favorited.ShouldBe(false);
    result.Article.FavoritesCount.ShouldBe(0);
  }

  [Fact]
  public async Task UnfavoriteArticle_WithoutAuthentication_ReturnsUnauthorized()
  {
    var createRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Unfavorite Unauthenticated Test",
        Description = "Description",
        Body = "Body"
      }
    };

    var (_, createResult) = await App.ArticlesUser1Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(createRequest);
    var slug = createResult.Article.Slug;

    var response = await App.Client.DeleteAsync($"/api/articles/{slug}/favorite", TestContext.Current.CancellationToken);

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task UnfavoriteArticle_WithNonExistentSlug_ReturnsNotFound()
  {
    var response = await App.ArticlesUser1Client.DeleteAsync("/api/articles/no-such-article/favorite", TestContext.Current.CancellationToken);

    response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
  }
}
