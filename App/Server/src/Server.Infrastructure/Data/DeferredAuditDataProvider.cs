using System.Collections.Concurrent;
using Audit.Core;

namespace Server.Infrastructure.Data;

/// <summary>
/// Custom Audit.NET data provider that buffers events per transaction.
/// Events are only written to file on transaction commit, discarded on rollback.
/// </summary>
public class DeferredAuditDataProvider : AuditDataProvider
{
  private static readonly AsyncLocal<string?> _currentTransactionId = new();
  private static readonly ConcurrentDictionary<string, List<AuditEvent>> _transactionBuffers = new();
  private static string _auditLogsPath = string.Empty;

  public static void SetFileLogProvider(string auditLogsPath)
  {
    _auditLogsPath = auditLogsPath;

    // Ensure the audit logs directory exists
    if (!Directory.Exists(auditLogsPath))
    {
      Directory.CreateDirectory(auditLogsPath);
    }
  }

  public override object InsertEvent(AuditEvent auditEvent)
  {
    BufferEvent(auditEvent);
    return Guid.NewGuid(); // Return a placeholder ID
  }

  public override Task<object> InsertEventAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default)
  {
    BufferEvent(auditEvent);
    return Task.FromResult<object>(Guid.NewGuid()); // Return a placeholder ID
  }

  public override void ReplaceEvent(object eventId, AuditEvent auditEvent)
  {
    // Not needed for our use case
  }

  public override Task ReplaceEventAsync(object eventId, AuditEvent auditEvent, CancellationToken cancellationToken = default)
  {
    // Not needed for our use case
    return Task.CompletedTask;
  }

  /// <summary>
  /// Sets the current transaction ID for the async context.
  /// </summary>
  public static void SetTransactionId(string transactionId)
  {
    _currentTransactionId.Value = transactionId;
  }

  /// <summary>
  /// Gets the current transaction ID from the async context.
  /// </summary>
  public static string? GetTransactionId()
  {
    return _currentTransactionId.Value;
  }

  /// <summary>
  /// Clears the current transaction ID.
  /// </summary>
  public static void ClearTransactionId()
  {
    _currentTransactionId.Value = null;
  }

  /// <summary>
  /// Buffers an audit event for the current transaction.
  /// </summary>
  private static void BufferEvent(AuditEvent auditEvent)
  {
    var transactionId = GetTransactionId();
    if (transactionId == null)
    {
      // No transaction context, write immediately
      WriteEventToFile(auditEvent);
      return;
    }

    var buffer = _transactionBuffers.GetOrAdd(transactionId, _ => new List<AuditEvent>());
    lock (buffer)
    {
      buffer.Add(auditEvent);
    }
  }

  /// <summary>
  /// Flushes all buffered events for the current transaction (on commit).
  /// </summary>
  public static void FlushTransaction()
  {
    var transactionId = GetTransactionId();
    if (transactionId == null)
    {
      return;
    }

    if (_transactionBuffers.TryRemove(transactionId, out var buffer))
    {
      lock (buffer)
      {
        foreach (var auditEvent in buffer)
        {
          WriteEventToFile(auditEvent);
        }
      }
    }

    ClearTransactionId();
  }

  /// <summary>
  /// Discards all buffered events for the current transaction (on rollback).
  /// </summary>
  public static void DiscardTransaction()
  {
    var transactionId = GetTransactionId();
    if (transactionId == null)
    {
      return;
    }

    _transactionBuffers.TryRemove(transactionId, out _);
    ClearTransactionId();
  }

  /// <summary>
  /// Writes an audit event to a file.
  /// </summary>
  private static void WriteEventToFile(AuditEvent auditEvent)
  {
    var fileName = $"audit_{DateTime.UtcNow:yyyyMMdd_HHmmss_fff}_{Guid.NewGuid():N}.json";
    var filePath = Path.Combine(_auditLogsPath, fileName);

    // Use Audit.NET's built-in JSON serialization
    var json = auditEvent.ToJson();
    File.WriteAllText(filePath, json);
  }
}
