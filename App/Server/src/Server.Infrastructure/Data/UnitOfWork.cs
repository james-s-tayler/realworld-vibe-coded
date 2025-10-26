using Ardalis.Result;

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

      // Set transaction ID for audit buffering
      var transactionId = transaction.TransactionId.ToString();
      DeferredAuditDataProvider.SetTransactionId(transactionId);

      try
      {
        _logger.LogDebug("Transaction started with ID: {TransactionId}", transactionId);

        // Execute the operation
        var result = await operation(cancellationToken);

        // Check if result is an Ardalis.Result<T> and handle accordingly
        var resultType = result?.GetType();
        if (resultType != null && resultType.IsGenericType &&
            resultType.GetGenericTypeDefinition() == typeof(Result<>))
        {
          // Use reflection to check IsSuccess property
          var isSuccessProperty = resultType.GetProperty("IsSuccess");
          var statusProperty = resultType.GetProperty("Status");
          var errorsProperty = resultType.GetProperty("Errors");
          var validationErrorsProperty = resultType.GetProperty("ValidationErrors");

          if (isSuccessProperty != null)
          {
            var isSuccess = (bool)(isSuccessProperty.GetValue(result) ?? false);

            if (isSuccess)
            {
              await transaction.CommitAsync(cancellationToken);
              _logger.LogDebug("Transaction committed - Result was successful");

              // Flush buffered audit events on successful commit
              DeferredAuditDataProvider.FlushTransaction();
            }
            else
            {
              await transaction.RollbackAsync(cancellationToken);

              // Discard buffered audit events on rollback
              DeferredAuditDataProvider.DiscardTransaction();

              var status = statusProperty?.GetValue(result)?.ToString() ?? "Unknown";
              var errors = errorsProperty?.GetValue(result) as IEnumerable<string> ?? Array.Empty<string>();
              var validationErrors = validationErrorsProperty?.GetValue(result);

              var validationErrorsStr = "none";
              if (validationErrors != null)
              {
                var validationErrorsList = validationErrors as IEnumerable<object>;
                if (validationErrorsList != null)
                {
                  validationErrorsStr = string.Join(", ", validationErrorsList.Select(e =>
                  {
                    var identifierProp = e.GetType().GetProperty("Identifier");
                    var errorMessageProp = e.GetType().GetProperty("ErrorMessage");
                    var identifier = identifierProp?.GetValue(e)?.ToString() ?? "";
                    var errorMessage = errorMessageProp?.GetValue(e)?.ToString() ?? "";
                    return $"{identifier}: {errorMessage}";
                  }));
                }
              }

              _logger.LogWarning("Transaction rolled back - Status: {Status}, Errors: {Errors}, ValidationErrors: {ValidationErrors}",
                status,
                string.Join(", ", errors),
                validationErrorsStr);
            }
          }
          else
          {
            // If we can't determine success, commit by default
            await transaction.CommitAsync(cancellationToken);
            DeferredAuditDataProvider.FlushTransaction();
            _logger.LogDebug("Transaction committed (IsSuccess property not found)");
          }
        }
        else
        {
          // For non-Result types, always commit
          await transaction.CommitAsync(cancellationToken);
          DeferredAuditDataProvider.FlushTransaction();
          _logger.LogDebug("Transaction committed");
        }

        return result;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Transaction failed with exception, rolling back");
        await transaction.RollbackAsync(cancellationToken);

        // Discard buffered audit events on exception rollback
        DeferredAuditDataProvider.DiscardTransaction();

        throw;
      }
    });
  }
}
