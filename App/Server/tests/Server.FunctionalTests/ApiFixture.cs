using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Sinks.XUnit3;
using Server.Web.DevOnly.Configuration;

namespace Server.FunctionalTests;

/// <summary>
/// Fixture class that provides Identity API helper methods for functional tests
/// </summary>
public class ApiFixture : AppFixture<Program>, IApiFixture
{
  private readonly FeatureFlagOverrideSource _overrideSource = new();

  public void SetTestOutputHelper(ITestOutputHelper testOutputHelper) =>
    Services.GetRequiredService<XUnit3TestOutputSink>().TestOutputHelper = testOutputHelper;

  public void ClearFeatureFlagOverrides() => _overrideSource.Provider.ClearAllOverrides();

  protected override void ConfigureServices(IServiceCollection services)
  {
    services.AddSingleton(Options.Create(new XUnit3TestOutputSinkOptions()));
    services.AddSingleton<XUnit3TestOutputSink>();
    services.AddSingleton(_overrideSource.Provider);
  }

  protected override IHost ConfigureAppHost(IHostBuilder builder)
  {
    builder.ConfigureAppConfiguration((_, config) =>
    {
      config.Add(_overrideSource);
    });

    builder.UseSerilog((ctx, serviceProvider, loggerConfiguration) =>
      loggerConfiguration
          .ReadFrom.Configuration(ctx.Configuration)
          .WriteTo.XUnit3TestOutput(
        serviceProvider.GetRequiredService<XUnit3TestOutputSink>()));

    return base.ConfigureAppHost(builder);
  }
}
