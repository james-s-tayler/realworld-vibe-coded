using Serilog.Core;
using Serilog.Events;

namespace Server.UnitTests.Infrastructure;

/// <summary>
/// Custom Serilog sink that writes to xUnit ITestOutputHelper.
/// Supports both xUnit v2 (Xunit.Abstractions.ITestOutputHelper) and xUnit v3 (Xunit.ITestOutputHelper).
/// </summary>
public class TestOutputSink : ILogEventSink
{
  private readonly object _testOutputHelper;
  private readonly IFormatProvider? _formatProvider;

  public TestOutputSink(object testOutputHelper, IFormatProvider? formatProvider = null)
  {
    _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
    _formatProvider = formatProvider;
  }

  public void Emit(LogEvent logEvent)
  {
    var message = logEvent.RenderMessage(_formatProvider);
    var level = logEvent.Level.ToString().ToUpperInvariant().PadRight(3).Substring(0, 3);
    var timestamp = logEvent.Timestamp.ToString("HH:mm:ss.fff");

    var formattedMessage = $"[{timestamp} {level}] {message}";

    if (logEvent.Exception != null)
    {
      formattedMessage += Environment.NewLine + logEvent.Exception.ToString();
    }

    try
    {
      // Try xUnit v2 (Xunit.Abstractions.ITestOutputHelper)
      if (_testOutputHelper is Xunit.Abstractions.ITestOutputHelper v2Helper)
      {
        v2Helper.WriteLine(formattedMessage);
        return;
      }

      // Try xUnit v3 (Xunit.ITestOutputHelper)
      var helperType = _testOutputHelper.GetType();
      var writeLineMethod = helperType.GetMethod("WriteLine", new[] { typeof(string) });
      if (writeLineMethod != null)
      {
        writeLineMethod.Invoke(_testOutputHelper, new object[] { formattedMessage });
        return;
      }
    }
    catch
    {
      // Ignore errors writing to test output (test may have finished)
    }
  }
}
