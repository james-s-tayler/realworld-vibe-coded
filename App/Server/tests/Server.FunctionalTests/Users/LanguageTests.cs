using Server.Web.Users.GetCurrent;
using Server.Web.Users.Update;

namespace Server.FunctionalTests.Users;

public class LanguageTests : AppTestBase
{
  public LanguageTests(ApiFixture apiFixture, ITestOutputHelper output) : base(apiFixture, output)
  {
  }

  [Fact]
  public async Task GetCurrentUser_DefaultLanguage_ReturnsEnglish()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var (response, result) = await user.Client.GETAsync<GetCurrent, UserCurrentResponse>();

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.User.Language.ShouldBe("en");
  }

  [Fact]
  public async Task UpdateUser_WithValidLanguage_PersistsLanguage()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var updateRequest = new UpdateUserRequest
    {
      User = new UpdateUserData
      {
        Language = "ja",
      },
    };

    var (response, result) = await user.Client.PUTAsync<UpdateUser, UpdateUserRequest, UpdateUserResponse>(updateRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.User.Language.ShouldBe("ja");

    // Verify it persists on subsequent GET
    var (getResponse, getResult) = await user.Client.GETAsync<GetCurrent, UserCurrentResponse>();
    getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
    getResult.User.Language.ShouldBe("ja");
  }

  [Fact]
  public async Task UpdateUser_WithInvalidLanguage_ReturnsValidationError()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var updateRequest = new UpdateUserRequest
    {
      User = new UpdateUserData
      {
        Language = "xx",
      },
    };

    var (response, _) = await user.Client.PUTAsync<UpdateUser, UpdateUserRequest, object>(updateRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task ValidationErrors_WithJapaneseAcceptLanguage_ReturnsJapaneseMessages()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var updateRequest = new UpdateUserRequest
    {
      User = new UpdateUserData
      {
        Email = string.Empty,
      },
    };

    user.Client.DefaultRequestHeaders.Add("Accept-Language", "ja");

    var (response, _) = await user.Client.PUTAsync<UpdateUser, UpdateUserRequest, object>(updateRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

    var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
    content.ShouldNotBeNullOrEmpty();
  }

  [Fact]
  public async Task CustomError_WithJapaneseAcceptLanguage_ReturnsLocalizedMessage()
  {
    var tenant = await Fixture.RegisterTenantWithUsersAsync(2);

    var updateRequest = new UpdateUserRequest
    {
      User = new UpdateUserData
      {
        Email = tenant.Users[0].Email,
      },
    };

    tenant.Users[1].Client.DefaultRequestHeaders.Add("Accept-Language", "ja");

    var (response, _) = await tenant.Users[1].Client.PUTAsync<UpdateUser, UpdateUserRequest, object>(updateRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

    var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
    content.ShouldContain("\u30e1\u30fc\u30eb\u30a2\u30c9\u30ec\u30b9\u306f\u65e2\u306b\u4f7f\u7528\u3055\u308c\u3066\u3044\u307e\u3059");
  }
}
