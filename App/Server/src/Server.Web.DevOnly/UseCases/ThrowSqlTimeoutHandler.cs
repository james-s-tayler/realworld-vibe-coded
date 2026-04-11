using Server.SharedKernel.MediatR;

namespace Server.Web.DevOnly.UseCases;

#pragma warning disable SRV015 // DevOnly test endpoint
public class ThrowSqlTimeoutHandler : IQueryHandler<ThrowSqlTimeoutQuery, Unit>
{
  public const string TimeoutMessage = "Execution Timeout Expired. The timeout period elapsed prior to completion of the operation or the server is not responding.";

  public Task<Result<Unit>> Handle(ThrowSqlTimeoutQuery request, CancellationToken cancellationToken)
  {
    throw new TimeoutException(TimeoutMessage);
  }
}
#pragma warning restore SRV015
