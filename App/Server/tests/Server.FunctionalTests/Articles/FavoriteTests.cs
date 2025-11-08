using Server.FunctionalTests.Articles.Fixture;
using Server.UseCases.Articles;
using Server.Web.Articles.Create;
using Server.Web.Articles.Favorite;

namespace Server.FunctionalTests.Articles;

[Collection("Articles Integration Tests")]
public class FavoriteTests(ArticlesFixture app) : TestBase<ArticlesFixture>
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
        Body = "Body",
      },
    };

    var (_, createResult) = await app.ArticlesUser1Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(createRequest);
    var slug = createResult.Article.Slug;

    var (response, result) = await app.ArticlesUser1Client.POSTAsync<Favorite, FavoriteArticleRequest, ArticleResponse>(new FavoriteArticleRequest { Slug = slug });

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
        Body = "Body",
      },
    };

    var (_, createResult) = await app.ArticlesUser1Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(createRequest);
    var slug = createResult.Article.Slug;

    await app.ArticlesUser1Client.POSTAsync<Favorite, FavoriteArticleRequest, ArticleResponse>(new FavoriteArticleRequest { Slug = slug });

    var (response, result) = await app.ArticlesUser1Client.POSTAsync<Favorite, FavoriteArticleRequest, ArticleResponse>(new FavoriteArticleRequest { Slug = slug });

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
        Body = "Body",
      },
    };

    var (_, createResult) = await app.ArticlesUser1Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(createRequest);
    var slug = createResult.Article.Slug;

    await app.ArticlesUser1Client.POSTAsync<Favorite, FavoriteArticleRequest, ArticleResponse>(new FavoriteArticleRequest { Slug = slug });

    var (response, result) = await app.ArticlesUser2Client.POSTAsync<Favorite, FavoriteArticleRequest, ArticleResponse>(new FavoriteArticleRequest { Slug = slug });

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
        Body = "Body",
      },
    };

    var (_, createResult) = await app.ArticlesUser1Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(createRequest);
    var slug = createResult.Article.Slug;

    var (response, _) = await app.Client.POSTAsync<Favorite, FavoriteArticleRequest, object>(new FavoriteArticleRequest { Slug = slug });

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task FavoriteArticle_WithNonExistentSlug_ReturnsNotFound()
  {
    var (response, _) = await app.ArticlesUser1Client.POSTAsync<Favorite, FavoriteArticleRequest, object>(new FavoriteArticleRequest { Slug = "no-such-article" });

    response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
  }
}
