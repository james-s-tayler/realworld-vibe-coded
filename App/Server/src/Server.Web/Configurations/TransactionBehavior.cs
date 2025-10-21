using Microsoft.EntityFrameworkCore;
using Server.Infrastructure.Data;

namespace Server.Web.Configurations;

public class TransactionBehavior<TRequest, TResponse>(
  AppDbContext dbContext,
  ILogger<TransactionBehavior<TRequest, TResponse>> logger)
  : IPipelineBehavior<TRequest, TResponse>
  where TRequest : notnull
{
  public async Task<TResponse> Handle(
    TRequest request,
    RequestHandlerDelegate<TResponse> next,
    CancellationToken cancellationToken)
  {
    // Only wrap Commands in transactions, not Queries
    if (!IsCommand(request))
    {
      return await next();
    }

    logger.LogInformation("Beginning transaction for {CommandName}", typeof(TRequest).Name);

    var strategy = dbContext.Database.CreateExecutionStrategy();

    return await strategy.ExecuteAsync(async () =>
    {
      await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

      try
      {
        var response = await next();

        // Check if the result indicates success
        if (IsSuccessResult(response))
        {
          await transaction.CommitAsync(cancellationToken);
          logger.LogInformation("Committed transaction for {CommandName}", typeof(TRequest).Name);
        }
        else
        {
          await transaction.RollbackAsync(cancellationToken);
          logger.LogInformation("Rolled back transaction for {CommandName} due to unsuccessful result", typeof(TRequest).Name);
        }

        return response;
      }
      catch (Exception ex)
      {
        await transaction.RollbackAsync(cancellationToken);
        logger.LogError(ex, "Rolled back transaction for {CommandName} due to exception", typeof(TRequest).Name);
        throw;
      }
    });
  }

  private static bool IsCommand(TRequest request)
  {
    // Check if the request implements ICommand interface from Ardalis.SharedKernel
    var requestType = request.GetType();
    return requestType.GetInterfaces()
      .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(Ardalis.SharedKernel.ICommand<>));
  }

  private static bool IsSuccessResult(TResponse response)
  {
    // Check if response is an Ardalis.Result and if it's successful
    if (response == null)
    {
      return false;
    }

    var responseType = response.GetType();

    // Check if it's a Result or Result<T>
    if (responseType.IsGenericType && responseType.GetGenericTypeDefinition().FullName == "Ardalis.Result.Result`1")
    {
      // It's Result<T>
      var isSuccessProperty = responseType.GetProperty("IsSuccess");
      if (isSuccessProperty != null)
      {
        return (bool)(isSuccessProperty.GetValue(response) ?? false);
      }
    }
    else if (responseType.FullName == "Ardalis.Result.Result")
    {
      // It's Result (non-generic)
      var isSuccessProperty = responseType.GetProperty("IsSuccess");
      if (isSuccessProperty != null)
      {
        return (bool)(isSuccessProperty.GetValue(response) ?? false);
      }
    }

    // If it's not a Result type, assume success
    return true;
  }
}
