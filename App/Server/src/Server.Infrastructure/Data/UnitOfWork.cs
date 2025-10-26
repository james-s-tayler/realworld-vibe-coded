using Ardalis.Result;
using Audit.Core;

namespace Server.Infrastructure.Data;

/// <summary>
/// Unit of Work implementation using EF Core DbContext.
/// Leverages EF Core execution strategy for automatic retries on transient failures.
/// Integrates AuditScope for transaction auditing.
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

  public async Task<Result<T>> ExecuteInTransactionAsync<T>(
    Func<CancellationToken, Task<Result<T>>> operation,
    CancellationToken cancellationToken = default)
  {
    // Use EF Core execution strategy for retry logic
    var strategy = _dbContext.Database.CreateExecutionStrategy();

    return await strategy.ExecuteAsync(async () =>
    {
      // Create audit scope for the transaction
      using var scope = await AuditScope.CreateAsync(new AuditScopeOptions
      {
        EventType = "Transaction",
        AuditEvent = new AuditEvent
        {
          CustomFields = new Dictionary<string, object>
          {
            { "TransactionStartTime", DateTime.UtcNow }
          }
        }
      });

      // Begin transaction
      await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

      try
      {
        _logger.LogDebug("Transaction started");

        // Execute the operation
        var result = await operation(cancellationToken);

        // Check if the result is successful
        if (result.IsSuccess)
        {
          // Commit transaction on success
          await transaction.CommitAsync(cancellationToken);
          scope.SetCustomField("TransactionStatus", "Committed");
          scope.SetCustomField("ResultStatus", result.Status.ToString());
          _logger.LogInformation("Transaction committed successfully with status: {Status}", result.Status);
        }
        else
        {
          // Rollback transaction on failure
          await transaction.RollbackAsync(cancellationToken);
          scope.SetCustomField("TransactionStatus", "RolledBack");
          scope.SetCustomField("ResultStatus", result.Status.ToString());
          scope.Discard();
          _logger.LogWarning("Transaction rolled back due to operation failure. Result: {@Result}", result);
        }

        return result;
      }
      catch (Exception ex)
      {
        // Rollback transaction on exception
        await transaction.RollbackAsync(cancellationToken);
        scope.SetCustomField("TransactionStatus", "RolledBack");
        scope.SetCustomField("Exception", ex.Message);
        scope.Discard();
        _logger.LogError(ex, "Transaction failed with exception, rolling back");
        throw;
      }
    });
  }
}
