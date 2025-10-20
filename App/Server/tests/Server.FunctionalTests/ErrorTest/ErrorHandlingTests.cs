using System.Net;
using System.Text.Json;
using Server.Web.ErrorTest;

namespace Server.FunctionalTests.ErrorTest;

/// <summary>
/// Tests for error handling at different layers of the application
/// Ensures consistent error response format across all error types
/// </summary>
[Collection("Error Test Integration Tests")]
public class ErrorHandlingTests(ErrorTestFixture App) : TestBase<ErrorTestFixture>
{
  [Fact]
  public async Task ThrowInEndpoint_ReturnsInternalServerError()
  {
    // Act
    var response = await App.Client.GetAsync("/api/error-test/throw-in-endpoint", TestContext.Current.CancellationToken);

    // Assert  
    response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
  }

  [Fact]
  public async Task ThrowInHandler_ReturnsBadRequest()
  {
    // Act
    var response = await App.Client.GetAsync("/api/error-test/throw-in-handler", TestContext.Current.CancellationToken);

    // Assert - exception caught by MediatR behavior and converted to Result.Error
    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task ReturnError_ReturnsBadRequest()
  {
    // Act
    var response = await App.Client.GetAsync("/api/error-test/return-error", TestContext.Current.CancellationToken);

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task AllErrorFormats_HaveErrorsField()
  {
    // This test verifies that all three error scenarios return a response with an "errors" field
    // that the frontend can reliably check

    // Act
    var endpointResponse = await App.Client.GetAsync("/api/error-test/throw-in-endpoint", TestContext.Current.CancellationToken);
    var handlerResponse = await App.Client.GetAsync("/api/error-test/throw-in-handler", TestContext.Current.CancellationToken);
    var resultResponse = await App.Client.GetAsync("/api/error-test/return-error", TestContext.Current.CancellationToken);

    // Assert - all should have errors field
    var endpointContent = await endpointResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
    var handlerContent = await handlerResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
    var resultContent = await resultResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

    var endpointPd = JsonSerializer.Deserialize<JsonElement>(endpointContent);
    var handlerPd = JsonSerializer.Deserialize<JsonElement>(handlerContent);
    var resultPd = JsonSerializer.Deserialize<JsonElement>(resultContent);

    // All should have "errors" property - this is the key consistency requirement
    endpointPd.TryGetProperty("errors", out _).ShouldBeTrue();
    handlerPd.TryGetProperty("errors", out _).ShouldBeTrue();
    resultPd.TryGetProperty("errors", out _).ShouldBeTrue();
  }
}
