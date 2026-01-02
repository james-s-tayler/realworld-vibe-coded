using System.Net.Http.Headers;

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
    var registerPayload = new
    {
      email,
      password,
    };

    var registerResponse = await fixture.Client.PostAsJsonAsync(
      "/api/identity/register",
      registerPayload,
      TestContext.Current.CancellationToken);

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
    var invitePayload = new
    {
      email,
      password,
    };

    var registerResponse = await owner.Client.PostAsJsonAsync(
      "/api/identity/invite",
      invitePayload,
      TestContext.Current.CancellationToken);

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
      }),
    };
  }

  private static async Task<string> LoginUserAsync(
    this AppFixture<Program> fixture,
    string email,
    string password)
  {
    var loginPayload = new
    {
      email,
      password,
    };

    var response = await fixture.Client.PostAsJsonAsync(
      "/api/identity/login?useCookies=false",
      loginPayload,
      TestContext.Current.CancellationToken);

    response.EnsureSuccessStatusCode();

    var result = await response.Content.ReadFromJsonAsync<IdentityLoginResponse>(TestContext.Current.CancellationToken);
    return result?.AccessToken ?? throw new InvalidOperationException("Login did not return an access token");
  }

  private record IdentityLoginResponse(string AccessToken);
}
