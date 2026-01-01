using System.Net.Http.Headers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Sinks.XUnit3;

namespace Server.FunctionalTests;

/// <summary>
/// Base fixture class that provides Identity API helper methods for test fixtures
/// </summary>
public abstract class ApiFixtureBase : AppFixture<Program>
{
  public async Task<string> RegisterUserAsync(
    string email,
    string password,
    CancellationToken cancellationToken = default)
  {
    var registerPayload = new
    {
      email,
      password,
    };

    // Step 1: Register the user (returns empty body with 200 OK on success)
    var registerResponse = await Client.PostAsJsonAsync(
      "/api/identity/register",
      registerPayload,
      cancellationToken);

    if (!registerResponse.IsSuccessStatusCode)
    {
      var errorContent = await registerResponse.Content.ReadAsStringAsync(cancellationToken);
      throw new InvalidOperationException($"Registration failed with status {registerResponse.StatusCode}. Response: {errorContent}");
    }

    // Step 2: Login to get the access token (Identity API register doesn't return a token)
    return await LoginUserAsync(email, password, cancellationToken);
  }

  public async Task<(HttpClient Client, string Email, string AccessToken)> RegisterUserAndCreateClientAsync(
    string? email = null,
    string? password = null,
    CancellationToken cancellationToken = default)
  {
    email ??= $"user-{Guid.NewGuid()}@example.com";
    password ??= "Password123!";

    var accessToken = await RegisterUserAsync(email, password, cancellationToken);
#pragma warning disable SRV007
    var client = CreateAuthenticatedClient(accessToken);
#pragma warning restore SRV007

    return (client, email, accessToken);
  }

  public async Task<string> LoginUserAsync(
    string email,
    string password,
    CancellationToken cancellationToken = default)
  {
    var loginPayload = new
    {
      email,
      password,
    };

    var response = await Client.PostAsJsonAsync(
      "/api/identity/login?useCookies=false",
      loginPayload,
      cancellationToken);

    response.EnsureSuccessStatusCode();

    var result = await response.Content.ReadFromJsonAsync<IdentityLoginResponse>(cancellationToken);
    return result?.AccessToken ?? throw new InvalidOperationException("Login did not return an access token");
  }

  public HttpClient CreateAuthenticatedClient(string accessToken)
  {
    return CreateClient(c =>
    {
      c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    });
  }

  public void SetTestOutputHelper(ITestOutputHelper testOutputHelper) =>
    Services.GetRequiredService<XUnit3TestOutputSink>().TestOutputHelper = testOutputHelper;

  protected override void ConfigureServices(IServiceCollection services)
  {
    services.AddSingleton(Options.Create(new XUnit3TestOutputSinkOptions()));
    services.AddSingleton<XUnit3TestOutputSink>();
  }

  protected override IHost ConfigureAppHost(IHostBuilder builder)
  {
    builder.UseSerilog((_, serviceProvider, loggerConfiguration) =>
      loggerConfiguration.WriteTo.XUnit3TestOutput(
        serviceProvider.GetRequiredService<XUnit3TestOutputSink>()));

    return base.ConfigureAppHost(builder);
  }

  private record IdentityLoginResponse(string AccessToken);
}
