namespace Server.Infrastructure.Data;

public static class SeedData
{
  public static async Task InitializeAsync(IdentityDbContext identityDbContext, DomainDbContext domainDbContext)
  {
    // No seed data currently needed
    await Task.CompletedTask;
  }

  public static async Task PopulateTestDataAsync(IdentityDbContext identityDbContext, DomainDbContext domainDbContext)
  {
    // No test data currently needed
    await Task.CompletedTask;
  }
}
