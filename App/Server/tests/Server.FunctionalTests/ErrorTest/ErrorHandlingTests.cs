using System.Text.Json;

namespace Server.FunctionalTests.ErrorTest;

[Collection("Error Test Integration Tests")]
public class ErrorHandlingTests(ErrorTestFixture App)
{
  /// <summary>
  /// Helper to parse error response from problem details format
  /// </summary>
  private async Task<ErrorResponse> ParseErrorResponse(HttpResponseMessage response)
  {
    var content = await response.Content.ReadAsStringAsync();
    var problemDetails = JsonSerializer.Deserialize<ProblemDetailsResponse>(content, new JsonSerializerOptions
    {
      PropertyNameCaseInsensitive = true
    });

    return new ErrorResponse
    {
      StatusCode = response.StatusCode,
      ProblemDetails = problemDetails!
    };
  }

  [Fact]
  public async Task ValidationError_ReturnsCorrectFormat()
  {
    // Act
    var response = await App.Client.GetAsync("/api/error-test/validation-error", TestContext.Current.CancellationToken);

    // Assert - FastEndpoints returns 400 for validation errors
    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

    var errorResponse = await ParseErrorResponse(response);
    errorResponse.ProblemDetails.ShouldNotBeNull();
    errorResponse.ProblemDetails.Status.ShouldBe(400);
    errorResponse.ProblemDetails.Errors.ShouldNotBeNull();
    // Should have at least 1 validation error
    errorResponse.ProblemDetails.Errors.Count.ShouldBeGreaterThanOrEqualTo(1);
  }

  [Fact]
  public async Task ThrowInUseCase_ReturnsCorrectFormat()
  {
    // Act
    var response = await App.Client.GetAsync("/api/error-test/throw-in-use-case", TestContext.Current.CancellationToken);

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);

    var errorResponse = await ParseErrorResponse(response);
    errorResponse.ProblemDetails.ShouldNotBeNull();
    errorResponse.ProblemDetails.Status.ShouldBe(500);
    errorResponse.ProblemDetails.Errors.ShouldNotBeNull();
    // Should have at least one error with the exception type
    errorResponse.ProblemDetails.Errors.Count.ShouldBeGreaterThanOrEqualTo(1);
  }

  [Fact]
  public async Task ThrowInEndpoint_ReturnsCorrectFormat()
  {
    // Act
    var response = await App.Client.GetAsync("/api/error-test/throw-in-endpoint", TestContext.Current.CancellationToken);

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);

    var errorResponse = await ParseErrorResponse(response);
    errorResponse.ProblemDetails.ShouldNotBeNull();
    errorResponse.ProblemDetails.Status.ShouldBe(500);
    errorResponse.ProblemDetails.Errors.ShouldNotBeNull();
    // Should have at least one error with the exception type
    errorResponse.ProblemDetails.Errors.Count.ShouldBeGreaterThanOrEqualTo(1);
  }

  [Fact]
  public async Task AllErrorTypes_UseSameFormat()
  {
    // Arrange & Act
    var validationResponse = await App.Client.GetAsync("/api/error-test/validation-error", TestContext.Current.CancellationToken);
    var useCaseResponse = await App.Client.GetAsync("/api/error-test/throw-in-use-case", TestContext.Current.CancellationToken);
    var endpointResponse = await App.Client.GetAsync("/api/error-test/throw-in-endpoint", TestContext.Current.CancellationToken);

    // Assert - All should return problem details in the same structure
    var validationError = await ParseErrorResponse(validationResponse);
    var useCaseError = await ParseErrorResponse(useCaseResponse);
    var endpointError = await ParseErrorResponse(endpointResponse);

    // All should have the same structure
    validationError.ProblemDetails.Type.ShouldNotBeNull();
    useCaseError.ProblemDetails.Type.ShouldNotBeNull();
    endpointError.ProblemDetails.Type.ShouldNotBeNull();

    validationError.ProblemDetails.Title.ShouldNotBeNull();
    useCaseError.ProblemDetails.Title.ShouldNotBeNull();
    endpointError.ProblemDetails.Title.ShouldNotBeNull();

    validationError.ProblemDetails.Status.ShouldNotBeNull();
    useCaseError.ProblemDetails.Status.ShouldNotBeNull();
    endpointError.ProblemDetails.Status.ShouldNotBeNull();

    validationError.ProblemDetails.Errors.ShouldNotBeNull();
    useCaseError.ProblemDetails.Errors.ShouldNotBeNull();
    endpointError.ProblemDetails.Errors.ShouldNotBeNull();

    // The critical error responses from use case and endpoint should be identical in format
    useCaseError.ProblemDetails.Status.ShouldBe(500);
    endpointError.ProblemDetails.Status.ShouldBe(500);
    useCaseError.ProblemDetails.Type.ShouldBe(endpointError.ProblemDetails.Type);
    useCaseError.ProblemDetails.Title.ShouldBe(endpointError.ProblemDetails.Title);
  }
}

/// <summary>
/// Response object for error handling
/// </summary>
public class ErrorResponse
{
  public HttpStatusCode StatusCode { get; set; }
  public ProblemDetailsResponse ProblemDetails { get; set; } = null!;
}

/// <summary>
/// Problem details response structure
/// </summary>
public class ProblemDetailsResponse
{
  public string Type { get; set; } = null!;
  public string Title { get; set; } = null!;
  public int? Status { get; set; }
  public List<ErrorDetail> Errors { get; set; } = new();
}

/// <summary>
/// Error detail structure
/// </summary>
public class ErrorDetail
{
  public string Name { get; set; } = null!;
  public string Reason { get; set; } = null!;
}
