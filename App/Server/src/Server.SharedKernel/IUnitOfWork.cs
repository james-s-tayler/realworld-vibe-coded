namespace Server.SharedKernel;

/// <summary>
/// Defines a unit of work pattern for managing database transactions.
/// Implementations should leverage EF Core execution strategies for automatic retries.
/// </summary>
public interface IUnitOfWork
{
  /// <summary>
  /// Executes a function within a transaction context using EF Core execution strategy.
  /// </summary>
  /// <typeparam name="TResult">The result type</typeparam>
  /// <param name="operation">The operation to execute within the transaction</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>The result of the operation</returns>
  Task<TResult> ExecuteInTransactionAsync<TResult>(
    Func<CancellationToken, Task<TResult>> operation,
    CancellationToken cancellationToken = default);
}
