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
    var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(content, new JsonSerializerOptions
    {
      PropertyNameCaseInsensitive = true
    });

    problemDetails.ShouldNotBeNull();
    problemDetails.Status.ShouldBe(400);
    problemDetails.Errors.ShouldNotBeNull();
    problemDetails.Errors.Count().ShouldBeGreaterThanOrEqualTo(1);
  }

  [Fact]
  public async Task ThrowInUseCase_ReturnsCorrectFormat()
  {
    // Act
    var response = await App.Client.GetAsync("/api/error-test/throw-in-use-case", TestContext.Current.CancellationToken);

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);

    var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
    var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(content, new JsonSerializerOptions
    {
      PropertyNameCaseInsensitive = true
    });

    problemDetails.ShouldNotBeNull();
    problemDetails.Status.ShouldBe(500);
    problemDetails.Errors.ShouldNotBeNull();
    problemDetails.Errors.Count().ShouldBeGreaterThanOrEqualTo(1);
  }

  [Fact]
  public async Task ThrowInEndpoint_ReturnsCorrectFormat()
  {
    // Act
    var response = await App.Client.GetAsync("/api/error-test/throw-in-endpoint", TestContext.Current.CancellationToken);

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);

    var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
    var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(content, new JsonSerializerOptions
    {
      PropertyNameCaseInsensitive = true
    });

    problemDetails.ShouldNotBeNull();
    problemDetails.Status.ShouldBe(500);
    problemDetails.Errors.ShouldNotBeNull();
    problemDetails.Errors.Count().ShouldBeGreaterThanOrEqualTo(1);
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

    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    var validationError = JsonSerializer.Deserialize<ProblemDetails>(validationContent, options);
    var useCaseError = JsonSerializer.Deserialize<ProblemDetails>(useCaseContent, options);
    var endpointError = JsonSerializer.Deserialize<ProblemDetails>(endpointContent, options);

    // All should have the same structure with type, title, status, and errors properties
    validationError.ShouldNotBeNull();
    useCaseError.ShouldNotBeNull();
    endpointError.ShouldNotBeNull();

    validationError.Type.ShouldNotBeNull();
    useCaseError.Type.ShouldNotBeNull();
    endpointError.Type.ShouldNotBeNull();

    validationError.Title.ShouldNotBeNull();
    useCaseError.Title.ShouldNotBeNull();
    endpointError.Title.ShouldNotBeNull();

    validationError.Status.ShouldBeGreaterThan(0);
    useCaseError.Status.ShouldBeGreaterThan(0);
    endpointError.Status.ShouldBeGreaterThan(0);

    validationError.Errors.ShouldNotBeNull();
    useCaseError.Errors.ShouldNotBeNull();
    endpointError.Errors.ShouldNotBeNull();

    // The critical error responses from use case and endpoint should be identical in format
    useCaseError.Status.ShouldBe(500);
    endpointError.Status.ShouldBe(500);
    useCaseError.Type.ShouldBe(endpointError.Type);
    useCaseError.Title.ShouldBe(endpointError.Title);
  }
}
