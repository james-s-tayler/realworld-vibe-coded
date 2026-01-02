using Server.UseCases.Articles;
using Server.Web.Articles.Create;
using Server.Web.Articles.Favorite;

namespace Server.FunctionalTests.Articles;

public class FavoriteTests : AppTestBase
{
  public FavoriteTests(ApiFixture apiFixture, ITestOutputHelper output) : base(apiFixture, output)
  {
  }

  [Fact]
  public async Task FavoriteArticle_WithAuthentication_ReturnsArticleWithFavorite()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var createRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Favorite Article Test",
        Description = "Description",
        Body = "Body",
      },
    };

    var (_, createResult) = await user.Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(createRequest);
    var slug = createResult.Article.Slug;

    var (response, result) = await user.Client.POSTAsync<Favorite, FavoriteArticleRequest, ArticleResponse>(new FavoriteArticleRequest { Slug = slug });

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Article.ShouldNotBeNull();
    result.Article.Slug.ShouldBe(slug);
    result.Article.Favorited.ShouldBe(true);
    result.Article.FavoritesCount.ShouldBe(1);
  }

  [Fact]
  public async Task FavoriteArticle_AlreadyFavorited_ReturnsArticleWithFavorite()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var createRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Already Favorited Test",
        Description = "Description",
        Body = "Body",
      },
    };

    var (_, createResult) = await user.Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(createRequest);
    var slug = createResult.Article.Slug;

    await user.Client.POSTAsync<Favorite, FavoriteArticleRequest, ArticleResponse>(new FavoriteArticleRequest { Slug = slug });

    var (response, result) = await user.Client.POSTAsync<Favorite, FavoriteArticleRequest, ArticleResponse>(new FavoriteArticleRequest { Slug = slug });

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Article.Favorited.ShouldBe(true);
    result.Article.FavoritesCount.ShouldBe(1);
  }

  [Fact]
  public async Task FavoriteArticle_ByDifferentUser_IncreasesFavoritesCount()
  {
    // Arrange
    var tenant = await Fixture.RegisterTenantWithUsersAsync(2);

    var createRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Multiple Favorites Test",
        Description = "Description",
        Body = "Body",
      },
    };

    var (_, createResult) = await tenant.Users[0].Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(createRequest);
    var slug = createResult.Article.Slug;

    // Act
    await tenant.Users[0].Client.POSTAsync<Favorite, FavoriteArticleRequest, ArticleResponse>(new FavoriteArticleRequest { Slug = slug });

    var (response, result) = await tenant.Users[1].Client.POSTAsync<Favorite, FavoriteArticleRequest, ArticleResponse>(new FavoriteArticleRequest { Slug = slug });

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Article.Favorited.ShouldBe(true);
    result.Article.FavoritesCount.ShouldBe(2);
  }

  [Fact]
  public async Task FavoriteArticle_WithoutAuthentication_ReturnsUnauthorized()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var createRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Favorite Unauthenticated Test",
        Description = "Description",
        Body = "Body",
      },
    };

    var (_, createResult) = await user.Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(createRequest);
    var slug = createResult.Article.Slug;

    var (response, _) = await Fixture.Client.POSTAsync<Favorite, FavoriteArticleRequest, object>(new FavoriteArticleRequest { Slug = slug });

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task FavoriteArticle_WithNonExistentSlug_ReturnsNotFound()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var (response, _) = await user.Client.POSTAsync<Favorite, FavoriteArticleRequest, object>(new FavoriteArticleRequest { Slug = "no-such-article" });

    response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
  }
}
