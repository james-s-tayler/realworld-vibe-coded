#nullable disable
namespace Server.SharedKernel.Result;

public enum ResultStatus
{
  Ok,
  Created,
  Error,
  Forbidden,
  Unauthorized,
  Invalid,
  NotFound,
  NoContent,
  Conflict,
  CriticalError,
  Unavailable
}
