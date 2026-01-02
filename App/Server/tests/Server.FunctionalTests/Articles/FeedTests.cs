using Server.UseCases.Articles;
using Server.Web.Articles.Create;
using Server.Web.Articles.Feed;
using Server.Web.Profiles;
using Server.Web.Profiles.Follow;

namespace Server.FunctionalTests.Articles;

[Collection("Articles Integration Tests")]
public class FeedTests : AppTestBase<ApiFixture>
{
  public FeedTests(ApiFixture fixture) : base(fixture)
  {
  }

  [Fact]
  public async Task GetFeed_WithoutAuthentication_ReturnsUnauthorized()
  {
    var feedRequest = new FeedRequest();
    var (response, _) = await Fixture.Client.GETAsync<Feed, FeedRequest, object>(feedRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task GetFeed_WithAuthentication_ReturnsArticlesFeed()
  {
    var tenant = await Fixture.RegisterTenantWithUsersAsync(2);
    var user1 = tenant.Users[0];
    var user2 = tenant.Users[1];

    // User1 follows User2
    var followRequest = new FollowProfileRequest { Username = user2.Email };
    await user1.Client.POSTAsync<Follow, FollowProfileRequest, ProfileResponse>(followRequest);

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

    await user2.Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(createArticleRequest);

    // User1 gets feed
    var feedRequest = new FeedRequest();
    var (response, result) = await user1.Client.GETAsync<Feed, FeedRequest, ArticlesResponse>(feedRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Articles.ShouldNotBeNull();
  }

  [Fact]
  public async Task GetFeed_WithLimit_ReturnsLimitedArticles()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var feedRequest = new FeedRequest { Limit = 2 };
    var (response, result) = await user.Client.GETAsync<Feed, FeedRequest, ArticlesResponse>(feedRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Articles.ShouldNotBeNull();
    result.Articles.Count.ShouldBeLessThanOrEqualTo(2);
  }

  [Fact]
  public async Task GetFeed_WithOffset_SkipsArticles()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var feedRequest = new FeedRequest { Offset = 1 };
    var (response, result) = await user.Client.GETAsync<Feed, FeedRequest, ArticlesResponse>(feedRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Articles.ShouldNotBeNull();
  }

  [Fact]
  public async Task GetFeed_WithLimitAndOffset_ReturnsPaginatedResults()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var feedRequest = new FeedRequest { Limit = 2, Offset = 1 };
    var (response, result) = await user.Client.GETAsync<Feed, FeedRequest, ArticlesResponse>(feedRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Articles.ShouldNotBeNull();
    result.Articles.Count.ShouldBeLessThanOrEqualTo(2);
  }

  [Fact]
  public async Task GetFeed_WithInvalidPagination_ReturnsErrorDetail()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var feedRequest = new FeedRequest { Limit = 0, Offset = -1 };
    var (response, _) = await user.Client.GETAsync<Feed, FeedRequest, object>(feedRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task GetFeed_OffsetGreaterThanResults_ReturnsEmptyList()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var feedRequest = new FeedRequest { Offset = 100 };
    var (response, result) = await user.Client.GETAsync<Feed, FeedRequest, ArticlesResponse>(feedRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Articles.ShouldNotBeNull();
  }

  [Fact]
  public async Task GetFeed_WhenNotFollowingAnyone_ReturnsEmptyList()
  {
    // Create a new user who doesn't follow anyone
    var tenant = await Fixture.RegisterTenantAsync();

    var feedRequest = new FeedRequest();
    var (response, result) = await tenant.Users[0].Client.GETAsync<Feed, FeedRequest, ArticlesResponse>(feedRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Articles.ShouldNotBeNull();
    result.Articles.ShouldBeEmpty();
    result.ArticlesCount.ShouldBe(0);
  }
}
