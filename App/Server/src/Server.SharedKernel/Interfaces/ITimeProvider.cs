namespace Server.SharedKernel.Interfaces;

/// <summary>
/// Provides access to the current time. Allows for testable time-dependent logic.
/// </summary>
public interface ITimeProvider
{
  /// <summary>
  /// Gets the current UTC time.
  /// </summary>
  DateTime UtcNow { get; }
}
