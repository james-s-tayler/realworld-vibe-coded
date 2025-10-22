using System.Text.Json;

namespace Server.FunctionalTests.ErrorTest;

[Collection("Error Test Integration Tests")]
public class ErrorHandlingTests(ErrorTestFixture App)
{
  [Fact]
  public async Task ValidationError_ReturnsCorrectFormat()
  {
    // Act
    var response = await App.Client.GetAsync("/api/error-test/validation-error", TestContext.Current.CancellationToken);

    // Assert - FastEndpoints returns 400 for validation errors
    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

    var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
    var errorResponse = JsonSerializer.Deserialize<JsonElement>(content);

    errorResponse.GetProperty("status").GetInt32().ShouldBe(400);
    errorResponse.GetProperty("errors").GetArrayLength().ShouldBeGreaterThanOrEqualTo(1);
  }

  [Fact]
  public async Task ThrowInUseCase_ReturnsCorrectFormat()
  {
    // Act
    var response = await App.Client.GetAsync("/api/error-test/throw-in-use-case", TestContext.Current.CancellationToken);

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);

    var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
    var errorResponse = JsonSerializer.Deserialize<JsonElement>(content);

    errorResponse.GetProperty("status").GetInt32().ShouldBe(500);
    errorResponse.GetProperty("errors").GetArrayLength().ShouldBeGreaterThanOrEqualTo(1);
  }

  [Fact]
  public async Task ThrowInEndpoint_ReturnsCorrectFormat()
  {
    // Act
    var response = await App.Client.GetAsync("/api/error-test/throw-in-endpoint", TestContext.Current.CancellationToken);

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);

    var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
    var errorResponse = JsonSerializer.Deserialize<JsonElement>(content);

    errorResponse.GetProperty("status").GetInt32().ShouldBe(500);
    errorResponse.GetProperty("errors").GetArrayLength().ShouldBeGreaterThanOrEqualTo(1);
  }

  [Fact]
  public async Task AllErrorTypes_UseSameFormat()
  {
    // Arrange & Act
    var validationResponse = await App.Client.GetAsync("/api/error-test/validation-error", TestContext.Current.CancellationToken);
    var useCaseResponse = await App.Client.GetAsync("/api/error-test/throw-in-use-case", TestContext.Current.CancellationToken);
    var endpointResponse = await App.Client.GetAsync("/api/error-test/throw-in-endpoint", TestContext.Current.CancellationToken);

    // Assert - All should return problem details in the same structure
    var validationContent = await validationResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
    var useCaseContent = await useCaseResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
    var endpointContent = await endpointResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

    var validationError = JsonSerializer.Deserialize<JsonElement>(validationContent);
    var useCaseError = JsonSerializer.Deserialize<JsonElement>(useCaseContent);
    var endpointError = JsonSerializer.Deserialize<JsonElement>(endpointContent);

    // All should have the same structure with type, title, status, and errors properties
    validationError.TryGetProperty("type", out _).ShouldBeTrue();
    useCaseError.TryGetProperty("type", out _).ShouldBeTrue();
    endpointError.TryGetProperty("type", out _).ShouldBeTrue();

    validationError.TryGetProperty("title", out _).ShouldBeTrue();
    useCaseError.TryGetProperty("title", out _).ShouldBeTrue();
    endpointError.TryGetProperty("title", out _).ShouldBeTrue();

    validationError.TryGetProperty("status", out _).ShouldBeTrue();
    useCaseError.TryGetProperty("status", out _).ShouldBeTrue();
    endpointError.TryGetProperty("status", out _).ShouldBeTrue();

    validationError.TryGetProperty("errors", out _).ShouldBeTrue();
    useCaseError.TryGetProperty("errors", out _).ShouldBeTrue();
    endpointError.TryGetProperty("errors", out _).ShouldBeTrue();

    // The critical error responses from use case and endpoint should be identical in format
    useCaseError.GetProperty("status").GetInt32().ShouldBe(500);
    endpointError.GetProperty("status").GetInt32().ShouldBe(500);
    useCaseError.GetProperty("type").GetString().ShouldBe(endpointError.GetProperty("type").GetString());
    useCaseError.GetProperty("title").GetString().ShouldBe(endpointError.GetProperty("title").GetString());
  }
}
