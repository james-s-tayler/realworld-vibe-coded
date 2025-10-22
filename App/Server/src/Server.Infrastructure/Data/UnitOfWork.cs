namespace Server.Infrastructure.Data;

/// <summary>
/// Unit of Work implementation using EF Core DbContext.
/// Leverages EF Core execution strategy for automatic retries on transient failures.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
  private readonly AppDbContext _dbContext;
  private readonly ILogger<UnitOfWork> _logger;

  public UnitOfWork(AppDbContext dbContext, ILogger<UnitOfWork> logger)
  {
    _dbContext = dbContext;
    _logger = logger;
  }

  public async Task<TResult> ExecuteInTransactionAsync<TResult>(
    Func<CancellationToken, Task<TResult>> operation,
    CancellationToken cancellationToken = default)
  {
    // Use EF Core execution strategy for retry logic
    var strategy = _dbContext.Database.CreateExecutionStrategy();

    return await strategy.ExecuteAsync(async () =>
    {
      // Begin transaction
      await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

      try
      {
        _logger.LogDebug("Transaction started");

        // Execute the operation
        var result = await operation(cancellationToken);

        // Commit if successful
        await transaction.CommitAsync(cancellationToken);
        _logger.LogDebug("Transaction committed");

        return result;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Transaction failed, rolling back");
        await transaction.RollbackAsync(cancellationToken);
        throw;
      }
    });
  }
}
