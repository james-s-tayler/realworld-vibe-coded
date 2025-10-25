using Server.FunctionalTests.Articles.Fixture;
using Server.UseCases.Articles;
using Server.Web.Articles.Create;
using Server.Web.Articles.Feed;
using Server.Web.Profiles;
using Server.Web.Profiles.Follow;
using Server.Web.Users.Register;

namespace Server.FunctionalTests.Articles;

[Collection("Articles Integration Tests")]
public class FeedTests(ArticlesFixture App) : TestBase<ArticlesFixture>
{
  [Fact]
  public async Task GetFeed_WithoutAuthentication_ReturnsUnauthorized()
  {
    var feedRequest = new FeedRequest();
    var (response, _) = await App.Client.GETAsync<Feed, FeedRequest, object>(feedRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task GetFeed_WithAuthentication_ReturnsArticlesFeed()
  {
    // User1 follows User2
    var followRequest = new FollowProfileRequest { Username = App.ArticlesUser2Username };
    await App.ArticlesUser1Client.POSTAsync<Follow, FollowProfileRequest, ProfileResponse>(followRequest);

    // User2 creates an article
    var createArticleRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = $"Feed Test Article {Guid.NewGuid()}",
        Description = "Test Description",
        Body = "Test Body",
        TagList = new List<string> { "feedtest" }
      }
    };

    await App.ArticlesUser2Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(createArticleRequest);

    // User1 gets feed
    var feedRequest = new FeedRequest();
    var (response, result) = await App.ArticlesUser1Client.GETAsync<Feed, FeedRequest, ArticlesResponse>(feedRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Articles.ShouldNotBeNull();
  }

  [Fact]
  public async Task GetFeed_WithLimit_ReturnsLimitedArticles()
  {
    var feedRequest = new FeedRequest { Limit = 2 };
    var (response, result) = await App.ArticlesUser1Client.GETAsync<Feed, FeedRequest, ArticlesResponse>(feedRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Articles.ShouldNotBeNull();
    result.Articles.Count.ShouldBeLessThanOrEqualTo(2);
  }

  [Fact]
  public async Task GetFeed_WithOffset_SkipsArticles()
  {
    var feedRequest = new FeedRequest { Offset = 1 };
    var (response, result) = await App.ArticlesUser1Client.GETAsync<Feed, FeedRequest, ArticlesResponse>(feedRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Articles.ShouldNotBeNull();
  }

  [Fact]
  public async Task GetFeed_WithLimitAndOffset_ReturnsPaginatedResults()
  {
    var feedRequest = new FeedRequest { Limit = 2, Offset = 1 };
    var (response, result) = await App.ArticlesUser1Client.GETAsync<Feed, FeedRequest, ArticlesResponse>(feedRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Articles.ShouldNotBeNull();
    result.Articles.Count.ShouldBeLessThanOrEqualTo(2);
  }

  [Fact]
  public async Task GetFeed_WithInvalidPagination_ReturnsValidationError()
  {
    var feedRequest = new FeedRequest { Limit = 0, Offset = -1 };
    var (response, _) = await App.ArticlesUser1Client.GETAsync<Feed, FeedRequest, object>(feedRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task GetFeed_OffsetGreaterThanResults_ReturnsEmptyList()
  {
    var feedRequest = new FeedRequest { Offset = 100 };
    var (response, result) = await App.ArticlesUser1Client.GETAsync<Feed, FeedRequest, ArticlesResponse>(feedRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Articles.ShouldNotBeNull();
  }

  [Fact]
  public async Task GetFeed_WhenNotFollowingAnyone_ReturnsEmptyList()
  {
    // Create a new user who doesn't follow anyone
    var username = $"loner-{Guid.NewGuid()}";
    var email = $"loner-{Guid.NewGuid()}@example.com";
    var password = "password123";

    var registerRequest = new RegisterRequest
    {
      User = new UserData
      {
        Email = email,
        Username = username,
        Password = password
      }
    };

    var (_, registerResult) = await App.Client.POSTAsync<Register, RegisterRequest, RegisterResponse>(registerRequest);
    var token = registerResult.User.Token;

    var client = App.CreateClient(c =>
    {
      c.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Token", token);
    });

    var feedRequest = new FeedRequest();
    var (response, result) = await client.GETAsync<Feed, FeedRequest, ArticlesResponse>(feedRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Articles.ShouldNotBeNull();
    result.Articles.ShouldBeEmpty();
    result.ArticlesCount.ShouldBe(0);
  }
}
