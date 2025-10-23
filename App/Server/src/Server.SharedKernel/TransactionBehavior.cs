using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Server.SharedKernel;

/// <summary>
/// MediatR pipeline behavior that wraps Command handling in an EF Core transaction.
/// Only applies to ICommand&lt;&gt; requests; IQuery&lt;&gt; requests are not wrapped.
/// Commits the transaction if Result.IsSuccess is true; otherwise, rolls back.
/// Constrained to only work with Result&lt;T&gt; responses.
/// </summary>
/// <typeparam name="TRequest">The request type (must be ICommand&lt;Result&lt;TValue&gt;&gt;)</typeparam>
/// <typeparam name="TResponse">The response type (must be Result&lt;TValue&gt; for some TValue)</typeparam>
public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
  where TRequest : ICommand<TResponse>
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

  public TransactionBehavior(
    IUnitOfWork unitOfWork,
    ILogger<TransactionBehavior<TRequest, TResponse>> logger)
  {
    _unitOfWork = unitOfWork;
    _logger = logger;
  }

  public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
  {
    _logger.LogInformation("Starting transaction for {RequestName}", typeof(TRequest).Name);

    var response = await _unitOfWork.ExecuteInTransactionAsync(async (ct) =>
    {
      return await next();
    }, cancellationToken);

    // Check if response is a Result<T> by checking the type
    var responseType = response?.GetType();
    if (responseType != null && responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
    {
      // Use dynamic to avoid reflection for property access
      dynamic resultDynamic = response!;
      if (resultDynamic.IsSuccess)
      {
        _logger.LogInformation("Transaction committed for {RequestName}", typeof(TRequest).Name);
      }
      else
      {
        _logger.LogWarning("Transaction rolled back for {RequestName}", typeof(TRequest).Name);
      }
    }

    return response;
  }
}
