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

  public void SetTestOutputHelper(ITestOutputHelper testOutputHelper) =>
    Services.GetRequiredService<XUnit3TestOutputSink>().TestOutputHelper = testOutputHelper;

  protected override void ConfigureServices(IServiceCollection services)
  {
    services.AddSingleton(Options.Create(new XUnit3TestOutputSinkOptions()));
    services.AddSingleton<XUnit3TestOutputSink>();
  }

  protected override IHost ConfigureAppHost(IHostBuilder builder)
  {
    builder.UseSerilog((ctx, serviceProvider, loggerConfiguration) =>
      loggerConfiguration
          .ReadFrom.Configuration(ctx.Configuration)
          .WriteTo.XUnit3TestOutput(
        serviceProvider.GetRequiredService<XUnit3TestOutputSink>()));

    return base.ConfigureAppHost(builder);
  }

  private record IdentityLoginResponse(string AccessToken);
}
