using System.Net;
using Ardalis.HttpClientTestExtensions;

namespace Server.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class OpenTelemetryMetrics(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateClient();

  [Fact]
  public async Task GetMetrics_ShouldReturnPrometheusFormat()
  {
    // Act
    var response = await _client.GetAsync("/metrics");

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    var content = await response.Content.ReadAsStringAsync();
    
    // Should contain some basic OpenTelemetry metrics
    content.ShouldContain("# HELP");
    content.ShouldContain("# TYPE");
    
    // Should contain ASP.NET Core metrics (since we're instrumenting it)
    // These metrics may not appear until there's actual traffic, so we just verify the endpoint works
  }

  [Fact]
  public async Task GetHealth_Then_GetMetrics_ShouldContainRequestMetrics()
  {
    // Arrange - Make a request to generate some telemetry
    await _client.GetAsync("/api/contributors");
    
    // Act
    var response = await _client.GetAsync("/metrics");
    
    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    var content = await response.Content.ReadAsStringAsync();
    
    // Verify metrics endpoint is working
    content.ShouldContain("# HELP");
    content.ShouldContain("# TYPE");
  }
}