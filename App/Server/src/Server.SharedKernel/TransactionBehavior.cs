using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Server.SharedKernel;

/// <summary>
/// MediatR pipeline behavior that wraps Command handling in an EF Core transaction.
/// Only applies to ICommand&lt;&gt; requests; IQuery&lt;&gt; requests are not wrapped.
/// Commits the transaction if Result.IsSuccess is true; otherwise, rolls back.
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
    if (!IsCommand(request))
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

  private static bool IsCommand(TRequest request)
  {
    // Check if request implements ICommand<> using pattern matching instead of reflection
    return request is ICommand<TResponse>;
  }

  private static bool IsSuccessResult(TResponse response)
  {
    if (response == null)
    {
      return false;
    }

    // Check if response is IResult and has a successful status
    // Result<T> implements IResult and exposes Status property
    if (response is Ardalis.Result.IResult result)
    {
      // Success includes Ok, NoContent, and Created statuses
      return result.Status is ResultStatus.Ok or ResultStatus.NoContent or ResultStatus.Created;
    }

    return true; // Default to true for non-Result responses
  }
}
