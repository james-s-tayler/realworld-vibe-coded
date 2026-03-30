using System.Diagnostics;
using MediatR;
using Server.SharedKernel.Result;

namespace Server.SharedKernel.MediatR;

/// <summary>
/// MediatR pipeline behavior that creates OpenTelemetry spans for all requests.
/// Uses System.Diagnostics.ActivitySource (no OTel SDK dependency in SharedKernel).
/// </summary>
/// <typeparam name="TRequest">The request type implementing IResultRequest{T}</typeparam>
/// <typeparam name="T">The inner value type of Result{T}</typeparam>
public class TracingBehavior<TRequest, T> : IPipelineBehavior<TRequest, Result<T>>
  where TRequest : IResultRequest<T>
{
  private static readonly ActivitySource ActivitySource = new("Conduit.MediatR");

  public async Task<Result<T>> Handle(TRequest request, RequestHandlerDelegate<Result<T>> next, CancellationToken cancellationToken)
  {
    var requestName = typeof(TRequest).Name;
    var kind = request is ICommand<T> ? "Command" : "Query";

    using var activity = ActivitySource.StartActivity($"MediatR {kind}: {requestName}", ActivityKind.Internal);

    if (activity is null)
    {
      return await next(cancellationToken);
    }

    activity.SetTag("mediatr.request.name", requestName);
    activity.SetTag("mediatr.request.kind", kind);

    var result = await next(cancellationToken);

    activity.SetTag("mediatr.result.status", result.Status.ToString());

    if (!result.IsSuccess)
    {
      activity.SetStatus(ActivityStatusCode.Error, result.Status.ToString());
    }

    return result;
  }
}
