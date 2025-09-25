namespace Server.Infrastructure.Data;

public static class SeedData
{
  public static async Task InitializeAsync(AppDbContext dbContext)
  {
    // No seed data needed for the application
    await Task.CompletedTask;
  }

  public static async Task PopulateTestDataAsync(AppDbContext dbContext)
  {
    // No test data to populate
    await Task.CompletedTask;
  }
}
