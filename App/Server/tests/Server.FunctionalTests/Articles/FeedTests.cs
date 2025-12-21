using Server.FunctionalTests.Articles.Fixture;
using Server.UseCases.Articles;
using Server.Web.Articles.Create;
using Server.Web.Articles.Feed;
using Server.Web.Profiles;
using Server.Web.Profiles.Follow;

namespace Server.FunctionalTests.Articles;

[Collection("Articles Integration Tests")]
public class FeedTests(ArticlesFixture app) : TestBase<ArticlesFixture>
{
  [Fact]
  public async Task GetFeed_WithoutAuthentication_ReturnsUnauthorized()
  {
    var feedRequest = new FeedRequest();
    var (response, _) = await app.Client.GETAsync<Feed, FeedRequest, object>(feedRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task GetFeed_WithAuthentication_ReturnsArticlesFeed()
  {
    // User1 follows User2
    var followRequest = new FollowProfileRequest { Username = app.ArticlesUser2Username };
    await app.ArticlesUser1Client.POSTAsync<Follow, FollowProfileRequest, ProfileResponse>(followRequest);

    // User2 creates an article
    var createArticleRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = $"Feed Test Article {Guid.NewGuid()}",
        Description = "Test Description",
        Body = "Test Body",
        TagList = new List<string> { "feedtest" },
      },
    };

    await app.ArticlesUser2Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(createArticleRequest);

    // User1 gets feed
    var feedRequest = new FeedRequest();
    var (response, result) = await app.ArticlesUser1Client.GETAsync<Feed, FeedRequest, ArticlesResponse>(feedRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Articles.ShouldNotBeNull();
  }

  [Fact]
  public async Task GetFeed_WithLimit_ReturnsLimitedArticles()
  {
    var feedRequest = new FeedRequest { Limit = 2 };
    var (response, result) = await app.ArticlesUser1Client.GETAsync<Feed, FeedRequest, ArticlesResponse>(feedRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Articles.ShouldNotBeNull();
    result.Articles.Count.ShouldBeLessThanOrEqualTo(2);
  }

  [Fact]
  public async Task GetFeed_WithOffset_SkipsArticles()
  {
    var feedRequest = new FeedRequest { Offset = 1 };
    var (response, result) = await app.ArticlesUser1Client.GETAsync<Feed, FeedRequest, ArticlesResponse>(feedRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Articles.ShouldNotBeNull();
  }

  [Fact]
  public async Task GetFeed_WithLimitAndOffset_ReturnsPaginatedResults()
  {
    var feedRequest = new FeedRequest { Limit = 2, Offset = 1 };
    var (response, result) = await app.ArticlesUser1Client.GETAsync<Feed, FeedRequest, ArticlesResponse>(feedRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Articles.ShouldNotBeNull();
    result.Articles.Count.ShouldBeLessThanOrEqualTo(2);
  }

  [Fact]
  public async Task GetFeed_WithInvalidPagination_ReturnsErrorDetail()
  {
    var feedRequest = new FeedRequest { Limit = 0, Offset = -1 };
    var (response, _) = await app.ArticlesUser1Client.GETAsync<Feed, FeedRequest, object>(feedRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task GetFeed_OffsetGreaterThanResults_ReturnsEmptyList()
  {
    var feedRequest = new FeedRequest { Offset = 100 };
    var (response, result) = await app.ArticlesUser1Client.GETAsync<Feed, FeedRequest, ArticlesResponse>(feedRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Articles.ShouldNotBeNull();
  }

  [Fact]
  public async Task GetFeed_WhenNotFollowingAnyone_ReturnsEmptyList()
  {
    // Create a new user who doesn't follow anyone
    var email = $"loner-{Guid.NewGuid()}@example.com";
    var password = "password123";

    var (client, _, _) = await app.RegisterUserAndCreateClientAsync(email, password, TestContext.Current.CancellationToken);

    var feedRequest = new FeedRequest();
    var (response, result) = await client.GETAsync<Feed, FeedRequest, ArticlesResponse>(feedRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Articles.ShouldNotBeNull();
    result.Articles.ShouldBeEmpty();
    result.ArticlesCount.ShouldBe(0);
  }
}
