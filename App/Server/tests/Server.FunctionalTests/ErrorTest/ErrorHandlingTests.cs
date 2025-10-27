using System.Text.Json;
using Server.Web.DevOnly.Configuration;

namespace Server.FunctionalTests.ErrorTest;

[Collection("Error Test Integration Tests")]
public class ErrorHandlingTests(ErrorTestFixture app)
{
  [InlineData($"/{DevOnly.ROUTE}/{TestAuth.ROUTE}/throw-unauthorized", 401, 0, "authorization", "Unauthorized")]
  [InlineData($"/{DevOnly.ROUTE}/{TestError.ROUTE}/validation-error-validator", 400, 0, "name", "can't be blank")]
  [InlineData($"/{DevOnly.ROUTE}/{TestError.ROUTE}/validation-error-validator", 400, 1, "email", "must be a valid email address")]
  [InlineData($"/{DevOnly.ROUTE}/{TestError.ROUTE}/validation-error-endpoint", 400, 0, "field1", "This is a test validation error for field1")]
  [InlineData($"/{DevOnly.ROUTE}/{TestError.ROUTE}/validation-error-endpoint", 400, 1, "field2", "This is a test validation error for field2")]
  [InlineData($"/{DevOnly.ROUTE}/{TestError.ROUTE}/throw-in-use-case", 500, 0, "invalidOperationException", "This is a test exception thrown in the use case")]
  [InlineData($"/{DevOnly.ROUTE}/{TestError.ROUTE}/throw-in-endpoint", 500, 0, "invalidOperationException", "This is a test exception thrown in the endpoint")]
  [InlineData($"/{DevOnly.ROUTE}/{TestError.ROUTE}/throw-concurrency", 409, 0, "dbUpdateConcurrencyException", "Test concurrency conflict")]
  [Theory]
  public async Task AllErrorTypes_UseSameFormat(string route, int statusCode, int errorIndex, string name, string reason)
  {
    // Arrange & Act
    // SRV007: Using raw HttpClient.GetAsync is necessary here to test various error endpoints
    // that are specifically designed for error handling tests and don't have typed request/response DTOs.
#pragma warning disable SRV007
    var endpointResponse = await app.Client.GetAsync(route, TestContext.Current.CancellationToken);
#pragma warning restore SRV007

    // Assert - All should return problem details in the same structure
    var endpointContent = await endpointResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    var endpointError = JsonSerializer.Deserialize<ProblemDetails>(endpointContent, options);

    // All should have the same structure with type, title, status, and errors properties
    endpointError.ShouldNotBeNull();
    endpointError.Type.ShouldNotBeNull();
    endpointError.Title.ShouldNotBeNull();
    endpointError.Status.ShouldBe(statusCode);
    endpointError.Errors.ShouldNotBeEmpty();
    endpointError.Errors.ElementAt(errorIndex).Name.ShouldBe(name);
    endpointError.Errors.ElementAt(errorIndex).Reason.ShouldBe(reason);
  }

  [InlineData($"/{DevOnly.ROUTE}/{TestError.ROUTE}/validation-error-validator", 400, 0, "serializerErrors", "The JSON value could not be converted to Server.Web.DevOnly.Endpoints.TestValidationRequest. Path: $ | LineNumber: 0 | BytePositionInLine: 3.")]
  [Theory]
  public async Task TestDeserializationError(string route, int statusCode, int errorIndex, string name, string reason)
  {
    // Arrange & Act
    // SRV007: Using raw HttpClient.PostAsJsonAsync is necessary here to test deserialization error
    // handling with malformed JSON ("{"). FastEndpoints POSTAsync would not allow sending invalid JSON.
#pragma warning disable SRV007
    var endpointResponse = await app.Client.PostAsJsonAsync(route, "{", TestContext.Current.CancellationToken);
#pragma warning restore SRV007

    // Assert - All should return problem details in the same structure
    var endpointContent = await endpointResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    var endpointError = JsonSerializer.Deserialize<ProblemDetails>(endpointContent, options);

    // All should have the same structure with type, title, status, and errors properties
    endpointError.ShouldNotBeNull();
    endpointError.Type.ShouldNotBeNull();
    endpointError.Title.ShouldNotBeNull();
    endpointError.Status.ShouldBe(statusCode);
    endpointError.Errors.ShouldNotBeEmpty();
    endpointError.Errors.ElementAt(errorIndex).Name.ShouldBe(name);
    endpointError.Errors.ElementAt(errorIndex).Reason.ShouldBe(reason);
  }
}
