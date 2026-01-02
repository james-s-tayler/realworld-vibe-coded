using Server.UseCases.Articles;
using Server.UseCases.Tags;
using Server.Web.Articles.Create;
using Server.Web.Articles.List;
using Server.Web.Profiles.Get;
using Server.Web.Tags.List;
using Server.Web.Users.Update;

namespace Server.FunctionalTests;

public class MultiTenancyTests : AppTestBase
{
  public MultiTenancyTests(ApiFixture apiFixture, ITestOutputHelper output) : base(apiFixture, output)
  {
  }

  [Fact]
  public async Task Articles_AreIsolated_BetweenTenants()
  {
    // Arrange
    var tenant1 = await Fixture.RegisterTenantAsync();
    var tenant2 = await Fixture.RegisterTenantAsync();

    var articleRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Tenant 1 Article",
        Description = "Description for tenant 1",
        Body = "Body for tenant 1",
        TagList = new List<string> { "tenant1" },
      },
    };

    await tenant1.Users[0].Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(articleRequest);

    // Act
    var (response, result) = await tenant2.Users[0].Client.GETAsync<ListArticles, ArticlesResponse>();

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Articles.ShouldNotBeNull();
    result.Articles.ShouldBeEmpty();
    result.ArticlesCount.ShouldBe(0);
  }

  [Fact]
  public async Task Tags_AreIsolated_BetweenTenants()
  {
    // Arrange
    var tenant1 = await Fixture.RegisterTenantAsync();
    var tenant2 = await Fixture.RegisterTenantAsync();

    var articleRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Article with unique tags",
        Description = "Description",
        Body = "Body",
        TagList = new List<string> { "tenant1tag", "uniquetag" },
      },
    };

    await tenant1.Users[0].Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(articleRequest);

    // Act
    var (response, result) = await tenant2.Users[0].Client.GETAsync<List, TagsResponse>();

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Tags.ShouldNotBeNull();
    result.Tags.ShouldBeEmpty();
  }

  [Fact]
  public async Task Users_AreIsolated_BetweenTenants()
  {
    // Arrange
    var tenant1 = await Fixture.RegisterTenantAsync();
    var tenant2 = await Fixture.RegisterTenantAsync();

    var tenant1UserEmail = tenant1.Users[0].Email;

    // Act
    var getProfileRequest = new GetProfileRequest { Username = tenant1UserEmail };
    var (response, _) = await tenant2.Users[0].Client.GETAsync<Get, GetProfileRequest, object>(getProfileRequest);

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task DuplicateSlugs_AreAllowed_BetweenTenants()
  {
    // Arrange
    var tenant1 = await Fixture.RegisterTenantAsync();
    var tenant2 = await Fixture.RegisterTenantAsync();

    var articleRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Same Article Title",
        Description = "Description 1",
        Body = "Body 1",
      },
    };

    await tenant1.Users[0].Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(articleRequest);

    // Act
    var (response, result) = await tenant2.Users[0].Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(articleRequest);

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.Created);
    result.Article.ShouldNotBeNull();
    result.Article.Title.ShouldBe("Same Article Title");
    result.Article.Slug.ShouldBe("same-article-title");
  }

  [Fact]
  public async Task DuplicateUsernames_AreAllowed_BetweenTenants()
  {
    // Arrange
    var tenant1 = await Fixture.RegisterTenantAsync();
    var tenant2 = await Fixture.RegisterTenantAsync();

    var sharedUsername = $"shared-username-{Guid.NewGuid()}";

    var updateRequest1 = new UpdateUserRequest
    {
      User = new UpdateUserData
      {
        Username = sharedUsername,
      },
    };

    await tenant1.Users[0].Client.PUTAsync<UpdateUser, UpdateUserRequest, UpdateUserResponse>(updateRequest1);

    // Act
    var updateRequest2 = new UpdateUserRequest
    {
      User = new UpdateUserData
      {
        Username = sharedUsername,
      },
    };

    var (response, result) = await tenant2.Users[0].Client.PUTAsync<UpdateUser, UpdateUserRequest, UpdateUserResponse>(updateRequest2);

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.User.ShouldNotBeNull();
    result.User.Username.ShouldBe(sharedUsername);
  }
}
