using Server.FunctionalTests.Articles.Fixture;
using Server.UseCases.Articles;
using Server.Web.Articles.Create;

namespace Server.FunctionalTests.Articles;

[Collection("Articles Integration Tests")]
public class FavoriteTests(ArticlesFixture App) : TestBase<ArticlesFixture>
{
  [Fact]
  public async Task FavoriteArticle_WithAuthentication_ReturnsArticleWithFavorite()
  {
    var createRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Favorite Article Test",
        Description = "Description",
        Body = "Body"
      }
    };

    var (_, createResult) = await App.ArticlesUser1Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(createRequest);
    var slug = createResult.Article.Slug;

    var response = await App.ArticlesUser1Client.PostAsync($"/api/articles/{slug}/favorite", null, TestContext.Current.CancellationToken);
    var result = await response.Content.ReadFromJsonAsync<ArticleResponse>(cancellationToken: TestContext.Current.CancellationToken);
    result.ShouldNotBeNull();

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Article.ShouldNotBeNull();
    result.Article.Slug.ShouldBe(slug);
    result.Article.Favorited.ShouldBe(true);
    result.Article.FavoritesCount.ShouldBe(1);
  }

  [Fact]
  public async Task FavoriteArticle_AlreadyFavorited_ReturnsArticleWithFavorite()
  {
    var createRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Already Favorited Test",
        Description = "Description",
        Body = "Body"
      }
    };

    var (_, createResult) = await App.ArticlesUser1Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(createRequest);
    var slug = createResult.Article.Slug;

    await App.ArticlesUser1Client.PostAsync($"/api/articles/{slug}/favorite", null, TestContext.Current.CancellationToken);

    var response = await App.ArticlesUser1Client.PostAsync($"/api/articles/{slug}/favorite", null, TestContext.Current.CancellationToken);
    var result = await response.Content.ReadFromJsonAsync<ArticleResponse>(cancellationToken: TestContext.Current.CancellationToken);
    result.ShouldNotBeNull();

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Article.Favorited.ShouldBe(true);
    result.Article.FavoritesCount.ShouldBe(1);
  }

  [Fact]
  public async Task FavoriteArticle_ByDifferentUser_IncreasesFavoritesCount()
  {
    var createRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Multiple Favorites Test",
        Description = "Description",
        Body = "Body"
      }
    };

    var (_, createResult) = await App.ArticlesUser1Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(createRequest);
    var slug = createResult.Article.Slug;

    await App.ArticlesUser1Client.PostAsync($"/api/articles/{slug}/favorite", null, TestContext.Current.CancellationToken);

    var response = await App.ArticlesUser2Client.PostAsync($"/api/articles/{slug}/favorite", null, TestContext.Current.CancellationToken);
    var result = await response.Content.ReadFromJsonAsync<ArticleResponse>(cancellationToken: TestContext.Current.CancellationToken);
    result.ShouldNotBeNull();

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Article.Favorited.ShouldBe(true);
    result.Article.FavoritesCount.ShouldBe(2);
  }

  [Fact]
  public async Task FavoriteArticle_WithoutAuthentication_ReturnsUnauthorized()
  {
    var createRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Favorite Unauthenticated Test",
        Description = "Description",
        Body = "Body"
      }
    };

    var (_, createResult) = await App.ArticlesUser1Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(createRequest);
    var slug = createResult.Article.Slug;

    var response = await App.Client.PostAsync($"/api/articles/{slug}/favorite", null, TestContext.Current.CancellationToken);

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task FavoriteArticle_WithNonExistentSlug_ReturnsNotFound()
  {
    var response = await App.ArticlesUser1Client.PostAsync("/api/articles/no-such-article/favorite", null, TestContext.Current.CancellationToken);

    response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
  }
}
