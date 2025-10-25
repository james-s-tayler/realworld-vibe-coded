using Server.SharedKernel.Interfaces;

namespace Server.FunctionalTests;

/// <summary>
/// Test implementation of ITimeProvider that allows controllable time for testing.
/// </summary>
public class TestTimeProvider : ITimeProvider
{
  private readonly Func<DateTime> _timeFunc;

  /// <summary>
  /// Creates a TestTimeProvider with a custom time function.
  /// </summary>
  /// <param name="timeFunc">Function that returns the current time. If null, returns DateTime.UtcNow.</param>
  public TestTimeProvider(Func<DateTime>? timeFunc = null)
  {
    _timeFunc = timeFunc ?? (() => DateTime.UtcNow);
  }

  public DateTime UtcNow => _timeFunc();
}
