namespace E2eTests;

/// <summary>
/// xUnit collection definition for E2E tests.
/// This enables sharing the ApiFixture across all test classes in the collection.
/// </summary>
[CollectionDefinition("E2E Tests")]
public class E2eTestCollection : ICollectionFixture<ApiFixture>
{
  // This class has no code, and is never created. Its purpose is simply
  // to be the place to apply [CollectionDefinition] and all the
  // ICollectionFixture<> interfaces.
}
