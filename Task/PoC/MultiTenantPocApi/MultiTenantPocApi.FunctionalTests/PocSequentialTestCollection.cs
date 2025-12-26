namespace MultiTenantPocApi.FunctionalTests;

/// <summary>
/// Test collection to ensure tests run sequentially and don't share state
/// </summary>
[CollectionDefinition("POC Sequential Tests", DisableParallelization = true)]
public class PocSequentialTestCollection
{
}
