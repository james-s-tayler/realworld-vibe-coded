using System.Net.Http.Headers;
using Server.Web.Identity.Invite;
using Server.Web.Identity.Login;
using Server.Web.Identity.Register;

namespace Server.FunctionalTests.Identity;

public class InviteTests : AppTestBase
{
  public InviteTests(ApiFixture apiFixture, ITestOutputHelper output) : base(apiFixture, output)
  {
  }

  [Fact]
  public async Task Invite_WithAdminRole_SuccessfullyInvitesUser()
  {
    var ownerEmail = $"owner-{Guid.NewGuid()}@example.com";
    var password = "Password123!";

    var registerRequest = new RegisterRequest
    {
      Email = ownerEmail,
      Password = password,
    };

    var (registerResponse, _) = await Fixture.Client.POSTAsync<Register, RegisterRequest, object>(registerRequest);
    registerResponse.EnsureSuccessStatusCode();

    var loginRequest = new LoginRequest
    {
      Email = ownerEmail,
      Password = password,
    };

    var (loginResponse, loginResult) = await Fixture.Client.POSTAsync<LoginRequest, LoginResponse>("/api/identity/login?useCookies=false", loginRequest);
    loginResponse.EnsureSuccessStatusCode();

    var ownerClient = Fixture.CreateClient(c =>
    {
      c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.AccessToken!);
    });

    var invitedEmail = $"invited-{Guid.NewGuid()}@example.com";
    var inviteRequest = new InviteRequest
    {
      Email = invitedEmail,
      Password = password,
    };

    var (inviteResponse, _) = await ownerClient.POSTAsync<Invite, InviteRequest, object>(inviteRequest);

    inviteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

    var invitedLoginRequest = new LoginRequest
    {
      Email = invitedEmail,
      Password = password,
    };

    var (invitedLoginResponse, invitedLoginResult) = await Fixture.Client.POSTAsync<LoginRequest, LoginResponse>("/api/identity/login?useCookies=false", invitedLoginRequest);

    invitedLoginResponse.EnsureSuccessStatusCode();
    invitedLoginResult.AccessToken.ShouldNotBeNullOrEmpty();
  }

  [Fact]
  public async Task Invite_WithNonAdminRole_ReturnsForbidden()
  {
    var tenant = await Fixture.RegisterTenantWithUsersAsync(2);
#pragma warning disable SRV007
    var invitedClient = tenant.Users[1].Client;
#pragma warning restore SRV007

    var anotherInvitedEmail = $"another-invited-{Guid.NewGuid()}@example.com";
    var password = "Password123!";
    var anotherInviteRequest = new InviteRequest
    {
      Email = anotherInvitedEmail,
      Password = password,
    };

    var (anotherInviteResponse, _) = await invitedClient.POSTAsync<Invite, InviteRequest, object>(anotherInviteRequest);

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
