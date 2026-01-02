using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Server.SharedKernel.Result;

namespace Server.SharedKernel.MediatR;

/// <summary>
/// MediatR pipeline behavior that catches exceptions during request handling
/// and transforms them into Result.CriticalError with ProblemDetails format.
/// This behavior uses the constrained generic parameter T from IResultRequest{T}
/// to call factory methods directly.
/// </summary>
/// <typeparam name="TRequest">The request type implementing IResultRequest{T}</typeparam>
/// <typeparam name="T">The inner value type of Result{T}</typeparam>
public class ExceptionHandlingBehavior<TRequest, T> : IPipelineBehavior<TRequest, Result<T>>
  where TRequest : IResultRequest<T>
{
  // https://learn.microsoft.com/en-us/sql/relational-databases/errors-events/database-engine-events-and-errors-2000-to-2999?view=sql-server-ver17
  private const int DuplicateKeyViolation = 2601;
  private const int UniqueConstraintViolation = 2627;

  private readonly ILogger<ExceptionHandlingBehavior<TRequest, T>> _logger;

  public ExceptionHandlingBehavior(ILogger<ExceptionHandlingBehavior<TRequest, T>> logger)
  {
    _logger = logger;
  }

  public async Task<Result<T>> Handle(TRequest request, RequestHandlerDelegate<Result<T>> next, CancellationToken cancellationToken)
  {
    try
    {
      return await next(cancellationToken);
    }
    catch (DbUpdateConcurrencyException ex)
    {
      _logger.LogWarning(ex, "Concurrency conflict occurred while processing {RequestName}", typeof(TRequest).Name);

      return Result<T>.Conflict(ex);
    }
    catch (DbUpdateException ex) when (IsDuplicateKey(ex))
    {
      _logger.LogWarning(ex, "Duplicate key conflict occurred while processing {RequestName}", typeof(TRequest).Name);
      return Result<T>.Conflict(ex);
    }
    catch (SqlException ex) when (IsDuplicateKey(ex))
    {
      _logger.LogWarning(ex, "Duplicate key conflict occurred while processing {RequestName}", typeof(TRequest).Name);
      return Result<T>.Conflict(ex);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "An unhandled exception occurred while processing {RequestName}", typeof(TRequest).Name);

      return Result<T>.CriticalError(ex);
    }
  }

  private static bool IsDuplicateKey(Exception ex)
  {
    if (ex is SqlException sqlEx && (sqlEx.Number is DuplicateKeyViolation or UniqueConstraintViolation))
    {
      return true;
    }

    // Fallback to message-based detection (as requested)
    for (Exception? cur = ex; cur is not null; cur = cur.InnerException)
    {
      if (cur.Message?.IndexOf("duplicate key", StringComparison.OrdinalIgnoreCase) >= 0)
      {
        return true;
      }
    }

    return false;
  }
}
