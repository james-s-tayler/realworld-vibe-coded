namespace Server.FunctionalTests.Infrastructure;

/// <summary>
/// xUnit collection definition for assembly-level SQL Server fixture.
/// All tests in this assembly will share the same SQL Server container.
/// </summary>
[CollectionDefinition("SqlServer Assembly Collection")]
public class SqlServerAssemblyCollection : ICollectionFixture<SqlServerAssemblyFixture>
{
  // This class is intentionally empty. It's used to apply [CollectionDefinition] and ICollectionFixture<>
}
