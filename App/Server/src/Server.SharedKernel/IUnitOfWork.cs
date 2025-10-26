using Ardalis.Result;

namespace Server.SharedKernel;

/// <summary>
/// Defines a unit of work pattern for managing database transactions.
/// Implementations should leverage EF Core execution strategies for automatic retries.
/// </summary>
public interface IUnitOfWork
{
  /// <summary>
  /// Executes a function within a transaction context using EF Core execution strategy.
  /// The operation must return an Ardalis.Result type for proper transaction handling.
  /// Commits on Result.IsSuccess, rolls back on failure.
  /// </summary>
  /// <typeparam name="T">The inner value type of Result{T}</typeparam>
  /// <param name="operation">The operation to execute within the transaction</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>The result of the operation</returns>
  Task<Result<T>> ExecuteInTransactionAsync<T>(
    Func<CancellationToken, Task<Result<T>>> operation,
    CancellationToken cancellationToken = default);
}
