using Server.SharedKernel.Interfaces;

namespace Server.Infrastructure.Services;

/// <summary>
/// Default implementation of ITimeProvider that returns the system time.
/// </summary>
public class UtcNowTimeProvider : ITimeProvider
{
  public DateTime UtcNow => DateTime.UtcNow;
}
