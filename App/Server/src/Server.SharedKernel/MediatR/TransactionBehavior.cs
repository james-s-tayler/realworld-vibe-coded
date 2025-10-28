using MediatR;
using Microsoft.Extensions.Logging;
using Server.SharedKernel.Persistence;
using Server.SharedKernel.Result;

namespace Server.SharedKernel.MediatR;

/// <summary>
/// MediatR pipeline behavior that wraps Command handling in an EF Core transaction.
/// Only applies to ICommand{T} requests; IQuery{T} requests are not wrapped.
/// Commits the transaction if Result.IsSuccess is true; otherwise, rolls back.
/// </summary>
/// <typeparam name="TRequest">The request type implementing IResultRequest{T}</typeparam>
/// <typeparam name="T">The inner value type of Result{T}</typeparam>
public class TransactionBehavior<TRequest, T> : IPipelineBehavior<TRequest, Result<T>>
  where TRequest : IResultRequest<T>
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly ILogger<TransactionBehavior<TRequest, T>> _logger;

  public TransactionBehavior(
    IUnitOfWork unitOfWork,
    ILogger<TransactionBehavior<TRequest, T>> logger)
  {
    _unitOfWork = unitOfWork;
    _logger = logger;
  }

  public async Task<Result<T>> Handle(TRequest request, RequestHandlerDelegate<Result<T>> next, CancellationToken cancellationToken)
  {
    // Only wrap commands in transactions, not queries
    if (request is not ICommand<T>)
    {
      return await next();
    }

    _logger.LogInformation("Starting transaction for {RequestName}", typeof(TRequest).Name);

    var response = await _unitOfWork.ExecuteInTransactionAsync(async (ct) =>
    {
      return await next();
    }, cancellationToken);

    if (response.IsSuccess)
    {
      _logger.LogInformation("Transaction committed for {RequestName}", typeof(TRequest).Name);
    }
    else
    {
      _logger.LogWarning("Transaction rolled back for {RequestName}", typeof(TRequest).Name);
    }

    return response;
  }
}
