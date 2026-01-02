using Server.UseCases.Articles;
using Server.Web.Articles.Create;
using Server.Web.Articles.Favorite;
using Server.Web.Articles.Unfavorite;

namespace Server.FunctionalTests.Articles;

public class UnfavoriteTests : AppTestBase
{
  public UnfavoriteTests(ApiFixture apiFixture, ITestOutputHelper output) : base(apiFixture, output)
  {
  }

  [Fact]
  public async Task UnfavoriteArticle_WithAuthentication_ReturnsArticleWithoutFavorite()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var createRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Unfavorite Article Test",
        Description = "Description",
        Body = "Body",
      },
    };

    var (_, createResult) = await user.Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(createRequest);
    var slug = createResult.Article.Slug;

    await user.Client.POSTAsync<Favorite, FavoriteArticleRequest, ArticleResponse>(new FavoriteArticleRequest { Slug = slug });

    var (response, result) = await user.Client.DELETEAsync<Unfavorite, UnfavoriteArticleRequest, ArticleResponse>(new UnfavoriteArticleRequest { Slug = slug });

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Article.ShouldNotBeNull();
    result.Article.Favorited.ShouldBe(false);
    result.Article.FavoritesCount.ShouldBe(0);
  }

  [Fact]
  public async Task UnfavoriteArticle_NotAlreadyFavorited_ReturnsArticleWithoutFavorite()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var createRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Not Favorited Test",
        Description = "Description",
        Body = "Body",
      },
    };

    var (_, createResult) = await user.Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(createRequest);
    var slug = createResult.Article.Slug;

    var (response, result) = await user.Client.DELETEAsync<Unfavorite, UnfavoriteArticleRequest, ArticleResponse>(new UnfavoriteArticleRequest { Slug = slug });

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Article.Favorited.ShouldBe(false);
    result.Article.FavoritesCount.ShouldBe(0);
  }

  [Fact]
  public async Task UnfavoriteArticle_WithoutAuthentication_ReturnsUnauthorized()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var createRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Unfavorite Unauthenticated Test",
        Description = "Description",
        Body = "Body",
      },
    };

    var (_, createResult) = await user.Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(createRequest);
    var slug = createResult.Article.Slug;

    var (response, _) = await Fixture.Client.DELETEAsync<Unfavorite, UnfavoriteArticleRequest, object>(new UnfavoriteArticleRequest { Slug = slug });

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task UnfavoriteArticle_WithNonExistentSlug_ReturnsNotFound()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var (response, _) = await user.Client.DELETEAsync<Unfavorite, UnfavoriteArticleRequest, object>(new UnfavoriteArticleRequest { Slug = "no-such-article" });

    response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
  }
}
