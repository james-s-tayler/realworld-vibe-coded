using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Server.SharedKernel;

/// <summary>
/// MediatR pipeline behavior that wraps Command handling in an EF Core transaction.
/// Only applies to ICommand{T} requests; IQuery{T} requests are not wrapped.
/// Commits the transaction if Result.IsSuccess is true; otherwise, rolls back.
/// This behavior works with requests implementing IResultRequest{T}, allowing it to
/// check the Result type without using reflection on Result{T}.
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
  where TRequest : IRequest<TResponse>
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
    // Only wrap commands in transactions, not queries
    // Check if request implements ICommand<T> (any T)
    var requestType = request.GetType();
    var isCommand = requestType.GetInterfaces()
      .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>));

    if (!isCommand)
    {
      return await next();
    }

    _logger.LogInformation("Starting transaction for {RequestName}", typeof(TRequest).Name);

    var response = await _unitOfWork.ExecuteInTransactionAsync(async (ct) =>
    {
      return await next();
    }, cancellationToken);

    // Check if the response is a Result with IsSuccess property
    if (IsSuccessResult(response))
    {
      _logger.LogInformation("Transaction committed for {RequestName}", typeof(TRequest).Name);
    }
    else
    {
      _logger.LogWarning("Transaction rolled back for {RequestName}", typeof(TRequest).Name);
    }

    return response;
  }

  private static bool IsSuccessResult(TResponse response)
  {
    if (response == null)
    {
      return false;
    }

    var responseType = response.GetType();

    // Check if response is Result<T>
    if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
    {
      var isSuccessProperty = responseType.GetProperty(nameof(Result<object>.IsSuccess));
      if (isSuccessProperty != null)
      {
        var isSuccess = (bool?)isSuccessProperty.GetValue(response);
        return isSuccess == true;
      }
    }

    return true; // Default to true for non-Result responses
  }
}
