using System.Net.Http.Headers;
using Server.Web.Identity.Invite;
using Server.Web.Identity.Login;
using Server.Web.Identity.Register;

namespace Server.FunctionalTests;

public static class TenantFactory
{
  public static async Task<RegisteredTenant> RegisterTenantAsync(
    this AppFixture<Program> fixture)
  {
    return await fixture.RegisterTenantWithUsersAsync(1);
  }

  public static async Task<RegisteredTenant> RegisterTenantWithUsersAsync(
    this AppFixture<Program> fixture,
    int numUsers)
  {
    if (numUsers < 1)
    {
      throw new ArgumentOutOfRangeException(nameof(numUsers));
    }

    var tenant = new RegisteredTenant();

    for (var i = 0; i < numUsers; i++)
    {
      var email = $"user-{Guid.NewGuid()}@example.com";
      var password = "Password123!";

      if (i == 0)
      {
        tenant.Users.Add(await fixture.RegisterTenantUserAsync(email, password));
      }
      else
      {
        tenant.Users.Add(await fixture.InviteUserAsync(tenant.GetTenantOwner(), email, password));
      }
    }

    return tenant;
  }

  private static async Task<RegisteredUser> RegisterTenantUserAsync(
    this AppFixture<Program> fixture,
    string email,
    string password)
  {
    var registerRequest = new RegisterRequest
    {
      Email = email,
      Password = password,
    };

    var (registerResponse, _) = await fixture.Client.POSTAsync<Register, RegisterRequest, object>(registerRequest);

    if (!registerResponse.IsSuccessStatusCode)
    {
      var errorContent = await registerResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
      throw new InvalidOperationException($"Registration failed with status {registerResponse.StatusCode}. Response: {errorContent}");
    }

    var accessToken = await fixture.LoginUserAsync(email, password);

    return new RegisteredUser
    {
      Email = email,
      Client = fixture.CreateClient(c =>
      {
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        if (TestContext.Current.Test != null)
        {
          c.DefaultRequestHeaders.Add("x-correlation-id", TestContext.Current.Test.TestDisplayName);
        }
      }),
    };
  }

  private static async Task<RegisteredUser> InviteUserAsync(
    this AppFixture<Program> fixture,
    RegisteredUser owner,
    string email,
    string password)
  {
    var inviteRequest = new InviteRequest
    {
      Email = email,
      Password = password,
    };

    var (inviteResponse, _) = await owner.Client.POSTAsync<Invite, InviteRequest, object>(inviteRequest);

    if (!inviteResponse.IsSuccessStatusCode)
    {
      var errorContent = await inviteResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
      throw new InvalidOperationException($"Invite failed with status {inviteResponse.StatusCode}. Response: {errorContent}");
    }

    var accessToken = await fixture.LoginUserAsync(email, password);

    return new RegisteredUser
    {
      Email = email,
      Client = fixture.CreateClient(c =>
      {
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
      }),
    };
  }

  private static async Task<string> LoginUserAsync(
    this AppFixture<Program> fixture,
    string email,
    string password)
  {
    var loginRequest = new LoginRequest
    {
      Email = email,
      Password = password,
    };

    var (response, result) = await fixture.Client.POSTAsync<LoginRequest, LoginResponse>("/api/identity/login?useCookies=false", loginRequest);

    response.EnsureSuccessStatusCode();

    return result.AccessToken ?? throw new InvalidOperationException("Login did not return an access token");
  }
}
