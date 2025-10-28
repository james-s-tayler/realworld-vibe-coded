using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Server.SharedKernel.Ardalis.Result;
using Server.SharedKernel.Result;

namespace Server.SharedKernel.MediatR;

/// <summary>
/// MediatR pipeline behavior that catches exceptions during request handling
/// and transforms them into Result.CriticalError with ProblemDetails format.
/// This behavior uses the constrained generic parameter T from IResultRequest{T}
/// to call factory methods directly without reflection.
/// </summary>
/// <typeparam name="TRequest">The request type implementing IResultRequest{T}</typeparam>
/// <typeparam name="T">The inner value type of Result{T}</typeparam>
public class ExceptionHandlingBehavior<TRequest, T> : IPipelineBehavior<TRequest, Result<T>>
  where TRequest : IResultRequest<T>
{
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

      return CustomArdalisResultFactory.Conflict<T>(ex);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "An unhandled exception occurred while processing {RequestName}", typeof(TRequest).Name);

      return CustomArdalisResultFactory.CriticalError<T>(ex);
    }
  }
}
