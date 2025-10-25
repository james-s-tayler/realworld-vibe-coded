using Serilog.Exceptions;

namespace Server.Web.Configurations;

public static class LoggerConfigs
{
  public static WebApplicationBuilder AddLoggerConfigs(this WebApplicationBuilder builder)
  {

    builder.Host.UseSerilog((context, services, config) => config
      .ReadFrom.Configuration(builder.Configuration)
      .ReadFrom.Services(services)
      .Enrich.WithClientIp()
      .Enrich.WithCorrelationId()
      .Enrich.WithExceptionDetails()
      .Destructure.ToMaximumDepth(5)
      .Destructure.ToMaximumStringLength(10000)
      .Destructure.ToMaximumCollectionCount(10));

    return builder;
  }
}
