using Server.SharedKernel.Interfaces;

namespace Server.FunctionalTests;

/// <summary>
/// Test implementation of ITimeProvider that returns the system time.
/// In more advanced scenarios, this could be replaced with a controllable fake.
/// </summary>
public class TestTimeProvider : ITimeProvider
{
  public DateTime UtcNow => DateTime.UtcNow;
}
