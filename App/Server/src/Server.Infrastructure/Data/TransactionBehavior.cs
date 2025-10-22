using Ardalis.Result;
using MediatR;

namespace Server.Infrastructure.Data;

/// <summary>
/// MediatR pipeline behavior that wraps Command handling in an EF Core transaction.
/// Only applies to ICommand&lt;&gt; requests; IQuery&lt;&gt; requests are not wrapped.
/// Commits the transaction if Result.IsSuccess is true; otherwise, rolls back.
/// Catches DbUpdateConcurrencyException and returns Result.Conflict.
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

    try
    {
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
    catch (DbUpdateConcurrencyException ex)
    {
      _logger.LogWarning(ex, "Concurrency conflict occurred for {RequestName}", typeof(TRequest).Name);

      // Return a Conflict result if the response type supports it
      return CreateConflictResult(ex);
    }
  }

  private static bool IsCommand(TRequest request)
  {
    // Check if request implements ICommand<>
    var requestType = request.GetType();
    var interfaces = requestType.GetInterfaces();

    return interfaces.Any(i =>
      i.IsGenericType &&
      i.GetGenericTypeDefinition() == typeof(ICommand<>));
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

  private TResponse CreateConflictResult(DbUpdateConcurrencyException ex)
  {
    var responseType = typeof(TResponse);

    // Check if response is Result<T>
    if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
    {
      var valueType = responseType.GetGenericArguments()[0];
      var resultType = typeof(Result<>).MakeGenericType(valueType);

      // Create using Activator with Conflict status
      var result = Activator.CreateInstance(
        resultType,
        BindingFlags.NonPublic | BindingFlags.Instance,
        null,
        new object[] { ResultStatus.Conflict },
        null
      )!;

      // Set the error message via the Errors property
      var errorsProp = resultType.GetProperty(nameof(Result<object>.Errors))!;
      var errorMessage = "A concurrency conflict occurred. The data has been modified by another process.";
      errorsProp.SetValue(result, new[] { errorMessage });

      return (TResponse)result;
    }

    // If we can't create a Conflict result, rethrow the exception
    throw ex;
  }
}
