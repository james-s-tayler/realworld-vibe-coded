namespace Server.FunctionalTests;

[CollectionDefinition("Functional Tests")]
public class FunctionalTestCollection : ICollectionFixture<CustomWebApplicationFactory<Program>>
{
  // This class has no code, and is never created. Its purpose is simply
  // to be the place to apply [CollectionDefinition] and all the
  // ICollectionFixture<> interfaces.
}
