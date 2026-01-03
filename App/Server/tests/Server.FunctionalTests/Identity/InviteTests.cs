using Server.Web.Identity.Invite;

namespace Server.FunctionalTests.Identity;

public class InviteTests : AppTestBase
{
  public InviteTests(ApiFixture apiFixture, ITestOutputHelper output) : base(apiFixture, output)
  {
  }

  [Fact]
  public async Task Invite_WithNonAdminRole_ReturnsForbidden()
  {
    var tenant = await Fixture.RegisterTenantWithUsersAsync(2);

    var anotherInvitedEmail = $"another-invited-{Guid.NewGuid()}@example.com";
    var password = "Password123!";
    var anotherInviteRequest = new InviteRequest
    {
      Email = anotherInvitedEmail,
      Password = password,
    };

    var (anotherInviteResponse, _) = await tenant.Users[1].Client.POSTAsync<Invite, InviteRequest, object>(anotherInviteRequest);

    anotherInviteResponse.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
  }

  [Fact]
  public async Task Invite_WithoutAuthentication_ReturnsUnauthorized()
  {
    var inviteRequest = new InviteRequest
    {
      Email = $"test-{Guid.NewGuid()}@example.com",
      Password = "Password123!",
    };

    var (response, _) = await Fixture.Client.POSTAsync<Invite, InviteRequest, object>(inviteRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }
}
