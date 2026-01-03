using Server.Web.Infrastructure;

namespace Server.Web.Configurations;

public static class LoggerConfigs
{
  public static WebApplicationBuilder AddLoggerConfigs(this WebApplicationBuilder builder)
  {
    builder.Host.UseSerilog((context, services, config) => config
      .ReadFrom.Configuration(builder.Configuration)
      .Destructure.With<ApplicationUserDestructuringPolicy>());

    return builder;
  }
}
