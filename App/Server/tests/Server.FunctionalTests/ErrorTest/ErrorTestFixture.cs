using System.Text.Json;

namespace Server.FunctionalTests.ErrorTest;

/// <summary>
/// Fixture for error handling tests
/// </summary>
public class ErrorTestFixture : AppFixture<Program>
{
  // No special setup needed for error tests - use default configuration
}

[CollectionDefinition("Error Test Integration Tests")]
public class ErrorTestCollection : ICollectionFixture<ErrorTestFixture>
{
}
