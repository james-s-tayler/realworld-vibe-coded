using System.Text.Json;

namespace Server.FunctionalTests.ErrorTest;

[Collection("Error Test Integration Tests")]
public class ErrorHandlingTests(ErrorTestFixture App)
{
  [InlineData("/api/error-test/throw-unauthorized", 401, 0, "authorization", "Unauthorized")]
  [InlineData("/api/error-test/validation-error-validator", 400, 0, "name", "can't be blank")]
  [InlineData("/api/error-test/validation-error-validator", 400, 1, "email", "must be a valid email address")]
  [InlineData("/api/error-test/validation-error-endpoint", 400, 0, "field1", "This is a test validation error for field1")]
  [InlineData("/api/error-test/validation-error-endpoint", 400, 1, "field2", "This is a test validation error for field2")]
  [InlineData("/api/error-test/throw-in-use-case", 500, 0, "invalidOperationException", "This is a test exception thrown in the use case")]
  [InlineData("/api/error-test/throw-in-endpoint", 500, 0, "invalidOperationException", "This is a test exception thrown in the endpoint")]
  [Theory]
  public async Task AllErrorTypes_UseSameFormat(string route, int statusCode, int errorIndex, string name, string reason)
  {
    // Arrange & Act
    var endpointResponse = await App.Client.GetAsync(route, TestContext.Current.CancellationToken);

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
