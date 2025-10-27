using Ardalis.Result;
using Audit.Core;

namespace Server.Infrastructure.Data;

/// <summary>
/// Unit of Work implementation using EF Core DbContext.
/// Coordinates transactions between IdentityDbContext and DomainDbContext.
/// Leverages EF Core execution strategy for automatic retries on transient failures.
/// Integrates AuditScope for transaction auditing.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
  private readonly IdentityDbContext _identityDbContext;
  private readonly DomainDbContext _domainDbContext;
  private readonly ILogger<UnitOfWork> _logger;

  public UnitOfWork(
    IdentityDbContext identityDbContext,
    DomainDbContext domainDbContext,
    ILogger<UnitOfWork> logger)
  {
    _identityDbContext = identityDbContext;
    _domainDbContext = domainDbContext;
    _logger = logger;
  }

  public async Task<Result<T>> ExecuteInTransactionAsync<T>(
    Func<CancellationToken, Task<Result<T>>> operation,
    CancellationToken cancellationToken = default)
  {
    // Use EF Core execution strategy for retry logic
    // Use the domain context's strategy (both should use the same connection)
    var strategy = _domainDbContext.Database.CreateExecutionStrategy();

    return await strategy.ExecuteAsync(async () =>
    {
      // Begin transactions on both contexts
      // Note: These should share the same underlying database connection
      await using var identityTransaction = await _identityDbContext.Database.BeginTransactionAsync(cancellationToken);
      await using var domainTransaction = await _domainDbContext.Database.BeginTransactionAsync(cancellationToken);

      /*
       * NOTE: The following AuditScope logic is designed to mitigate the following problem.
       *       Essentially, when a rollback happens in EF Core, Audit.NET doesn't know about that and will write the audit trail anyway.
       *       This leads to a situation where your audit trail says something happened when it didn't.
       *
       *       So, how do we solve it? Well, there are a few potential ways. The TL;DR is it boils down to:
       *       - Find some way to buffer writes in memory until the transaction is committed or rolled back
       *       - Find some way to wire the EF Core and Audit.NET lifecycle together nicely
       *
       *       Both of these are technically doable with enough effort, but neither is super easy.
       *       Nor are they easy to reason about. So, why isn't this just simple?
       *
       *       AuditScope's in Audit.NET are independent by design with each scope managing its own lifecycle independently.
       *       AuditScopes do not nest. Audit.EntityFramework.Core produces audit logs of EventType "EntityFrameworkEvent".
       *       That library is in control of that AuditScope, not us.
       *       Therefore we can't simply call scope.Discard() here to not write the EntityFrameworkEvent.
       *       Believe me, I tried.
       *
       *       Hence the simple solution is:
       *       Rather than trying to get Audit.NET to NOT write a log, make it write an additional correlated log.
       *       By using a custom IAuditScopeFactory and IUserContext we can inject a stable correlation id into all logs and audit events.
       *       This can be used to correlate the state of the entity to what happened in the database transaction.
       *
       *       Assuming no logs got dropped of course... muahahaha!
       *       Ahhhh distributed systems!
       */
      await using var scope = await AuditScope.CreateAsync(new AuditScopeOptions { EventType = "DatabaseTransactionEvent" }, cancellationToken);

      try
      {
        _logger.LogDebug("Transaction started");

        // Execute the operation
        var result = await operation(cancellationToken);

        // Check if the result is successful
        if (result.IsSuccess)
        {
          // Commit both transactions on success
          await identityTransaction.CommitAsync(cancellationToken);
          await domainTransaction.CommitAsync(cancellationToken);
          scope.SetCustomField("TransactionStatus", "Committed");
          scope.SetCustomField("ResultStatus", result.Status.ToString());
          _logger.LogInformation("Transactions committed successfully with status: {Status}", result.Status);
        }
        else
        {
          // Rollback both transactions on failure
          await identityTransaction.RollbackAsync(cancellationToken);
          await domainTransaction.RollbackAsync(cancellationToken);
          scope.SetCustomField("TransactionStatus", "RolledBack");
          scope.SetCustomField("ResultStatus", result.Status.ToString());
          _logger.LogWarning("Transactions rolled back due to operation failure. Result: {@Result}", result);
        }

        return result;
      }
      catch (Exception ex)
      {
        // Rollback both transactions on exception
        await identityTransaction.RollbackAsync(cancellationToken);
        await domainTransaction.RollbackAsync(cancellationToken);
        scope.SetCustomField("TransactionStatus", "RolledBack");
        scope.SetCustomField("Exception", ex.Message);
        scope.Discard();
        _logger.LogError(ex, "Transactions failed with exception, rolling back");
        throw;
      }
    });
  }
}
